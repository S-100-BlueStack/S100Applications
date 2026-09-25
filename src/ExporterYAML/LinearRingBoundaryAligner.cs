#nullable enable

using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Valid;

namespace S100Framework.Topology.Geometry
{
    public static class LinearRingBoundaryAligner
    {
        /// <summary>
        /// Returns new rings whose convincingly coincident boundary runs use exactly the
        /// same coordinates and intermediate vertices.
        /// </summary>
        /// <remarks>
        /// The authoritative snap tolerance is <see cref="PrecisionModel.GridSize"/> from
        /// <paramref name="geometryFactory"/>. The method is intentionally conservative:
        /// a close coordinate is only used as an anchor, and a boundary run is aligned only
        /// after ordering, direction, overlap, distance, and ambiguity checks also succeed.
        /// </remarks>
        public static (LinearRing First, LinearRing Second) AlignNearlyCoincidentRings(
            LinearRing first,
            LinearRing second,
            GeometryFactory geometryFactory) {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);
            ArgumentNullException.ThrowIfNull(geometryFactory);

            return new Internal.LinearRingAlignmentOperation(
                first,
                second,
                geometryFactory).Execute();
        }
    }

    public sealed class RingAlignmentException : InvalidOperationException
    {
        public RingAlignmentException(string message)
            : base(message) {
        }
    }

}

namespace S100Framework.Topology.Geometry.Internal
{
    internal sealed class LinearRingAlignmentOperation
    {
        #region LinearRingAlignmentOperation
        // Candidate points must be within one precision-grid cell. This is the actual
        // topology snap tolerance and is never widened for point matching.
        private const double AnchorGridFactor = 1.0;

        // The supplied regression geometry contains a two-cell micro-segment at the end
        // of an otherwise coincident path. A two-cell envelope is therefore used only as
        // whole-path evidence after strong anchor, direction, overlap, and ordering checks.
        // It is not a second independently configurable snap tolerance.
        private const double PathEvidenceGridFactor = 2.0;

        private const double MinimumEvidenceLengthGridFactor = 8.0;
        private const double SignificantDirectionSegmentGridFactor = 4.0;
        private const double CandidateAmbiguityGridFactor = 0.20;
        private const double CompetingPathScoreGridFactor = 0.25;
        private const double RelativeLengthTolerance = 1E-4;
        private const double MachineEpsilon = 2.2204460492503131E-16;

        private static readonly double AnchorDirectionCosine =
            Math.Cos(10.0 * Math.PI / 180.0);

        private static readonly double PathDirectionCosine =
            Math.Cos(5.0 * Math.PI / 180.0);

        private readonly GeometryFactory _factory;
        private readonly PrecisionModel _precisionModel;
        private readonly double _tolerance;
        private readonly double _pathEvidenceTolerance;
        private readonly double _numericSlack;

        private readonly LinearRing _first;
        private readonly LinearRing _second;

        public LinearRingAlignmentOperation(
            LinearRing first,
            LinearRing second,
            GeometryFactory factory) {
            _first = first;
            _second = second;
            _factory = factory;
            _precisionModel = factory.PrecisionModel;
            _tolerance = _precisionModel.GridSize;

            if (!double.IsFinite(_tolerance) || _tolerance <= 0.0) {
                throw new ArgumentException(
                    "AlignNearlyCoincidentRings requires a fixed PrecisionModel with a positive GridSize.",
                    "geometryFactory");
            }

            _pathEvidenceTolerance = PathEvidenceGridFactor * _tolerance;

            var largestOrdinate = first.Coordinates
                .Concat(second.Coordinates)
                .SelectMany(static coordinate => new[]
                {
                    Math.Abs(coordinate.X),
                    Math.Abs(coordinate.Y),
                })
                .DefaultIfEmpty(1.0)
                .Max();

            // Include enough floating-point slack for an inclusive one-grid-cell test at
            // the magnitude of the input ordinates. For example, decimal coordinates one
            // grid cell apart can differ by a few ULPs after binary parsing.
            _numericSlack = Math.Max(
                _tolerance * 1E-9,
                Math.Min(
                    _tolerance * 1E-6,
                    16.0 * MachineEpsilon * Math.Max(1.0, largestOrdinate)));
        }

        public (LinearRing First, LinearRing Second) Execute() {
            var normalizedFirst = NormalizeAndValidateInput(_first, "first");
            var normalizedSecond = NormalizeAndValidateInput(_second, "second");

            var firstModel = new RingModel(normalizedFirst, _numericSlack);
            var secondModel = new RingModel(normalizedSecond, _numericSlack);

            var anchors = BuildAnchors(firstModel, secondModel);
            if (anchors.Count < 2) {
                return (normalizedFirst, normalizedSecond);
            }

            var intervals = BuildCandidateIntervals(firstModel, secondModel, anchors);
            if (intervals.Count == 0) {
                return (normalizedFirst, normalizedSecond);
            }

            var runs = BuildSharedRuns(firstModel, secondModel, anchors, intervals);
            if (runs.Count == 0) {
                return (normalizedFirst, normalizedSecond);
            }

            var firstReplacements = new List<Replacement>(runs.Count);
            var secondReplacements = new List<Replacement>(runs.Count);

            foreach (var run in runs) {
                firstReplacements.Add(new Replacement(
                    run.Start.First,
                    run.End.First,
                    run.CanonicalCoordinates,
                    run.CoversWholeRing));

                if (run.Orientation > 0) {
                    secondReplacements.Add(new Replacement(
                        run.Start.Second,
                        run.End.Second,
                        run.CanonicalCoordinates,
                        run.CoversWholeRing));
                }
                else {
                    secondReplacements.Add(new Replacement(
                        run.End.Second,
                        run.Start.Second,
                        ReversePath(run.CanonicalCoordinates),
                        run.CoversWholeRing));
                }
            }

            var alignedFirst = RebuildRing(firstModel, firstReplacements, "first");
            var alignedSecond = RebuildRing(secondModel, secondReplacements, "second");

            ValidateOutput(alignedFirst, "first");
            ValidateOutput(alignedSecond, "second");

            return (alignedFirst, alignedSecond);
        }

        private LinearRing NormalizeAndValidateInput(LinearRing source, string name) {
            var coordinates = new List<Coordinate>(source.NumPoints);

            foreach (var sourceCoordinate in source.Coordinates) {
                if (!double.IsFinite(sourceCoordinate.X) || !double.IsFinite(sourceCoordinate.Y)) {
                    throw new RingAlignmentException(
                        $"The {name} ring contains a non-finite coordinate.");
                }

                var precise = MakePrecise(sourceCoordinate);
                AppendExact(coordinates, precise);
            }

            // Work with one copy of the seam. Closure is restored explicitly below so
            // the first and final coordinate can never be processed independently.
            while (coordinates.Count > 1 && coordinates[0].Equals2D(coordinates[^1])) {
                coordinates.RemoveAt(coordinates.Count - 1);
            }

            if (coordinates.Count < 3) {
                throw new RingAlignmentException(
                    $"The {name} ring collapses below three distinct vertices on the configured precision grid.");
            }

            coordinates.Add(coordinates[0].Copy());
            var normalized = _factory.CreateLinearRing(coordinates.ToArray());
            ValidateOutput(normalized, $"precision-normalized {name} input");
            return normalized;
        }
        #endregion

        #region LinearRingAlignmentOperation.Models
        private enum AnchorKind
        {
            VertexVertex,
            FirstVertexToSecondSegment,
            SecondVertexToFirstSegment,
        }

        private sealed record VertexCandidate(double Distance, int TargetIndex);

        private sealed record SegmentCandidate(
            double Distance,
            int SegmentIndex,
            Location TargetLocation);

        private sealed class Anchor
        {
            public Anchor(
                Location first,
                Location second,
                double distance,
                AnchorKind kind) {
                First = first;
                Second = second;
                Distance = distance;
                Kind = kind;
            }

            public Location First { get; }
            public Location Second { get; }
            public double Distance { get; }
            public AnchorKind Kind { get; }
        }

        private sealed record CandidateInterval(
            int Index,
            Anchor Start,
            Anchor End,
            int Orientation,
            PathEvidence Evidence);

        private sealed record SharedRun(
            Anchor Start,
            Anchor End,
            int Orientation,
            IReadOnlyList<Coordinate> CanonicalCoordinates,
            bool CoversWholeRing);

        private sealed record Replacement(
            Location Start,
            Location End,
            IReadOnlyList<Coordinate> Coordinates,
            bool CoversWholeRing);

        private sealed record BasePathSelection(
            IReadOnlyList<Coordinate> BasePath,
            IReadOnlyList<Coordinate> OtherPath);

        private sealed record PathCost(double Maximum, double Mean);

        private sealed record PathEvidence(
            bool IsMatch,
            double MaximumDistance,
            double MeanDistance,
            double FirstLength,
            double SecondLength,
            double LengthDifference)
        {
            public static PathEvidence NoMatch { get; } = new(
                false,
                double.PositiveInfinity,
                double.PositiveInfinity,
                0.0,
                0.0,
                double.PositiveInfinity);
        }

        private sealed record PathVertex(
            double Measure,
            bool IsBaseVertex,
            int SourceIndex,
            Coordinate Coordinate);

        private sealed record SegmentProjection(
            Coordinate Coordinate,
            double Distance,
            double Fraction);

        private sealed record PathProjection(
            Coordinate Coordinate,
            double Distance,
            double Measure);

        private sealed record Location(
            int SegmentIndex,
            double Fraction,
            double Measure,
            Coordinate Coordinate,
            int? VertexIndex,
            double Distance = 0.0);

        private readonly record struct Direction(double X, double Y);

        private sealed class RingModel
        {
            private readonly double _numericSlack;
            private readonly double[] _cumulativeMeasures;

            public RingModel(LinearRing ring, double numericSlack) {
                Ring = ring;
                _numericSlack = numericSlack;
                Vertices = ring.Coordinates
                    .Take(ring.NumPoints - 1)
                    .Select(static coordinate => coordinate.Copy())
                    .ToArray();

                VertexCount = Vertices.Length;
                SegmentLengths = new double[VertexCount];
                _cumulativeMeasures = new double[VertexCount + 1];

                for (var index = 0; index < VertexCount; index++) {
                    SegmentLengths[index] = Vertices[index].Distance(
                        Vertices[(index + 1) % VertexCount]);
                    _cumulativeMeasures[index + 1] =
                        _cumulativeMeasures[index] + SegmentLengths[index];
                }

                Length = _cumulativeMeasures[^1];
            }

            public LinearRing Ring { get; }
            public Coordinate[] Vertices { get; }
            public int VertexCount { get; }
            public double[] SegmentLengths { get; }
            public double Length { get; }

            public Location VertexLocation(int vertexIndex) {
                return new Location(
                    vertexIndex,
                    0.0,
                    _cumulativeMeasures[vertexIndex],
                    Vertices[vertexIndex].Copy(),
                    vertexIndex);
            }

            public Location Project(Coordinate point, int segmentIndex) {
                var projection = ProjectPointToSegment(
                    point,
                    Vertices[segmentIndex],
                    Vertices[(segmentIndex + 1) % VertexCount]);

                return new Location(
                    segmentIndex,
                    projection.Fraction,
                    _cumulativeMeasures[segmentIndex] +
                    projection.Fraction * SegmentLengths[segmentIndex],
                    projection.Coordinate,
                    null,
                    projection.Distance);
            }

            public IEnumerable<Direction> IncidentDirections(int vertexIndex) {
                var previous = Vertices[(vertexIndex - 1 + VertexCount) % VertexCount];
                var current = Vertices[vertexIndex];
                var next = Vertices[(vertexIndex + 1) % VertexCount];

                yield return new Direction(
                    current.X - previous.X,
                    current.Y - previous.Y);
                yield return new Direction(
                    next.X - current.X,
                    next.Y - current.Y);
            }

            public double ForwardDistance(double fromMeasure, double toMeasure) {
                var distance = toMeasure - fromMeasure;
                if (distance < -_numericSlack) {
                    distance += Length;
                }
                else if (Math.Abs(distance) <= _numericSlack) {
                    return 0.0;
                }

                return distance;
            }

            public double CircularMeasureDistance(double first, double second) {
                return Math.Min(
                    ForwardDistance(first, second),
                    ForwardDistance(second, first));
            }

            public bool IsStrictlyBetweenForward(
                double startMeasure,
                double endMeasure,
                double candidateMeasure) {
                var totalDistance = ForwardDistance(startMeasure, endMeasure);
                var candidateDistance = ForwardDistance(startMeasure, candidateMeasure);

                return candidateDistance > _numericSlack &&
                       candidateDistance < totalDistance - _numericSlack;
            }

            public List<Coordinate> Slice(
                Location start,
                Location end,
                bool fullCycle = false) {
                var distance = fullCycle
                    ? Length
                    : ForwardDistance(start.Measure, end.Measure);

                if (distance <= _numericSlack) {
                    return new List<Coordinate>();
                }

                var result = new List<Coordinate> { start.Coordinate.Copy() };
                var endMeasure = start.Measure + distance;
                var segmentIndex = start.SegmentIndex;
                var nextVertexIndex = (segmentIndex + 1) % VertexCount;
                var nextVertexMeasure = _cumulativeMeasures[segmentIndex + 1];

                while (nextVertexMeasure < endMeasure - _numericSlack) {
                    AppendExact(result, Vertices[nextVertexIndex].Copy());
                    segmentIndex = nextVertexIndex;
                    nextVertexIndex = (segmentIndex + 1) % VertexCount;
                    nextVertexMeasure += SegmentLengths[segmentIndex];
                }

                AppendExact(result, end.Coordinate.Copy());
                return result;
            }
        }
        #endregion

        #region LinearRingAlignmentOperation.Geometry
        private bool VertexDirectionsAreCompatible(
            RingModel first,
            int firstVertexIndex,
            RingModel second,
            int secondVertexIndex) {
            foreach (var firstDirection in first.IncidentDirections(firstVertexIndex)) {
                foreach (var secondDirection in second.IncidentDirections(secondVertexIndex)) {
                    if (UndirectedCosine(
                            firstDirection.X,
                            firstDirection.Y,
                            secondDirection.X,
                            secondDirection.Y) >= AnchorDirectionCosine) {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool VertexAndSegmentDirectionsAreCompatible(
            RingModel source,
            int sourceVertexIndex,
            RingModel target,
            int targetSegmentIndex) {
            var targetStart = target.Vertices[targetSegmentIndex];
            var targetEnd = target.Vertices[(targetSegmentIndex + 1) % target.VertexCount];

            foreach (var sourceDirection in source.IncidentDirections(sourceVertexIndex)) {
                if (UndirectedCosine(
                        sourceDirection.X,
                        sourceDirection.Y,
                        targetEnd.X - targetStart.X,
                        targetEnd.Y - targetStart.Y) >= AnchorDirectionCosine) {
                    return true;
                }
            }

            return false;
        }

        private static double UndirectedCosine(
            double firstX,
            double firstY,
            double secondX,
            double secondY) {
            var firstLength = Math.Sqrt(firstX * firstX + firstY * firstY);
            var secondLength = Math.Sqrt(secondX * secondX + secondY * secondY);

            if (firstLength == 0.0 || secondLength == 0.0) {
                return 0.0;
            }

            return Math.Abs(
                (firstX * secondX + firstY * secondY) /
                (firstLength * secondLength));
        }

        private Coordinate MakePrecise(Coordinate source) {
            var result = source.Copy();
            _precisionModel.MakePrecise(result);
            return result;
        }

        private static double PathLength(IReadOnlyList<Coordinate> path) {
            var length = 0.0;
            for (var index = 0; index < path.Count - 1; index++) {
                length += path[index].Distance(path[index + 1]);
            }

            return length;
        }

        private static double[] PathMeasures(IReadOnlyList<Coordinate> path) {
            var result = new double[path.Count];
            for (var index = 1; index < path.Count; index++) {
                result[index] = result[index - 1] +
                                path[index - 1].Distance(path[index]);
            }

            return result;
        }

        private static double DistanceToPath(
            Coordinate point,
            IReadOnlyList<Coordinate> path) {
            var bestDistance = double.PositiveInfinity;

            for (var index = 0; index < path.Count - 1; index++) {
                var projection = ProjectPointToSegment(
                    point,
                    path[index],
                    path[index + 1]);
                bestDistance = Math.Min(bestDistance, projection.Distance);
            }

            return bestDistance;
        }

        private static PathProjection ProjectPointToPath(
            Coordinate point,
            IReadOnlyList<Coordinate> path) {
            var bestDistance = double.PositiveInfinity;
            var bestMeasure = 0.0;
            var bestSegment = -1;
            var bestFraction = 0.0;
            Coordinate? bestCoordinate = null;
            var cumulative = 0.0;

            for (var index = 0; index < path.Count - 1; index++) {
                var projection = ProjectPointToSegment(
                    point,
                    path[index],
                    path[index + 1]);
                var measure = cumulative + projection.Fraction *
                    path[index].Distance(path[index + 1]);

                var isBetter = projection.Distance < bestDistance;
                if (!isBetter && projection.Distance.Equals(bestDistance)) {
                    isBetter = index < bestSegment ||
                               (index == bestSegment && projection.Fraction < bestFraction);
                }

                if (isBetter) {
                    bestDistance = projection.Distance;
                    bestMeasure = measure;
                    bestSegment = index;
                    bestFraction = projection.Fraction;
                    bestCoordinate = projection.Coordinate;
                }

                cumulative += path[index].Distance(path[index + 1]);
            }

            return new PathProjection(
                bestCoordinate ?? point.Copy(),
                bestDistance,
                bestMeasure);
        }

        private static SegmentProjection ProjectPointToSegment(
            Coordinate point,
            Coordinate segmentStart,
            Coordinate segmentEnd) {
            var deltaX = segmentEnd.X - segmentStart.X;
            var deltaY = segmentEnd.Y - segmentStart.Y;
            var lengthSquared = deltaX * deltaX + deltaY * deltaY;

            var fraction = lengthSquared == 0.0
                ? 0.0
                : Math.Clamp(
                    ((point.X - segmentStart.X) * deltaX +
                     (point.Y - segmentStart.Y) * deltaY) /
                    lengthSquared,
                    0.0,
                    1.0);

            var projected = new Coordinate(
                segmentStart.X + fraction * deltaX,
                segmentStart.Y + fraction * deltaY);

            return new SegmentProjection(
                projected,
                point.Distance(projected),
                fraction);
        }

        private static List<Coordinate> ReversePath(
            IReadOnlyList<Coordinate> path) {
            return path
                .Reverse()
                .Select(static coordinate => coordinate.Copy())
                .ToList();
        }

        private static void RemoveConsecutiveDuplicates(List<Coordinate> coordinates) {
            for (var index = coordinates.Count - 1; index > 0; index--) {
                if (coordinates[index].Equals2D(coordinates[index - 1])) {
                    coordinates.RemoveAt(index);
                }
            }
        }

        private static void AppendExact(
            ICollection<Coordinate> coordinates,
            Coordinate coordinate) {
            if (coordinates is List<Coordinate> list) {
                if (list.Count == 0 || !list[^1].Equals2D(coordinate)) {
                    list.Add(coordinate);
                }

                return;
            }

            var last = coordinates.LastOrDefault();
            if (last is null || !last.Equals2D(coordinate)) {
                coordinates.Add(coordinate);
            }
        }

        private static int CompareCoordinates(Coordinate left, Coordinate right) {
            var xComparison = left.X.CompareTo(right.X);
            return xComparison != 0 ? xComparison : left.Y.CompareTo(right.Y);
        }

        private static int CompareCoordinateSequences(
            IReadOnlyList<Coordinate> left,
            IReadOnlyList<Coordinate> right) {
            var count = Math.Min(left.Count, right.Count);
            for (var index = 0; index < count; index++) {
                var comparison = CompareCoordinates(left[index], right[index]);
                if (comparison != 0) {
                    return comparison;
                }
            }

            return left.Count.CompareTo(right.Count);
        }
        #endregion

        #region LinearRingAlignmentOperation.Anchors
        private List<Anchor> BuildAnchors(RingModel first, RingModel second) {
            var firstVertexCandidates = CreateCandidateLists(first.VertexCount);
            var secondVertexCandidates = CreateCandidateLists(second.VertexCount);
            var anchorTolerance = AnchorGridFactor * _tolerance + _numericSlack;

            for (var firstIndex = 0; firstIndex < first.VertexCount; firstIndex++) {
                for (var secondIndex = 0; secondIndex < second.VertexCount; secondIndex++) {
                    var distance = first.Vertices[firstIndex].Distance(second.Vertices[secondIndex]);
                    if (distance > anchorTolerance ||
                        !VertexDirectionsAreCompatible(first, firstIndex, second, secondIndex)) {
                        continue;
                    }

                    firstVertexCandidates[firstIndex].Add(
                        new VertexCandidate(distance, secondIndex));
                    secondVertexCandidates[secondIndex].Add(
                        new VertexCandidate(distance, firstIndex));
                }
            }

            var firstChoices = firstVertexCandidates
                .Select(SelectUniqueVertexCandidate)
                .ToArray();
            var secondChoices = secondVertexCandidates
                .Select(SelectUniqueVertexCandidate)
                .ToArray();

            var firstHasVertexAnchor = new bool[first.VertexCount];
            var secondHasVertexAnchor = new bool[second.VertexCount];
            var anchors = new List<Anchor>();

            for (var firstIndex = 0; firstIndex < firstChoices.Length; firstIndex++) {
                var firstChoice = firstChoices[firstIndex];
                if (firstChoice is null) {
                    continue;
                }

                var secondIndex = firstChoice.TargetIndex;
                var secondChoice = secondChoices[secondIndex];
                if (secondChoice is null || secondChoice.TargetIndex != firstIndex) {
                    continue;
                }

                anchors.Add(new Anchor(
                    first.VertexLocation(firstIndex),
                    second.VertexLocation(secondIndex),
                    firstChoice.Distance,
                    AnchorKind.VertexVertex));

                firstHasVertexAnchor[firstIndex] = true;
                secondHasVertexAnchor[secondIndex] = true;
            }

            AddVertexToSegmentAnchors(
                source: first,
                target: second,
                sourceHasVertexAnchor: firstHasVertexAnchor,
                sourceIsFirst: true,
                anchors);

            AddVertexToSegmentAnchors(
                source: second,
                target: first,
                sourceHasVertexAnchor: secondHasVertexAnchor,
                sourceIsFirst: false,
                anchors);

            return DeduplicateAnchors(first, second, anchors);
        }

        private static List<VertexCandidate>[] CreateCandidateLists(int count) {
            var result = new List<VertexCandidate>[count];
            for (var index = 0; index < result.Length; index++) {
                result[index] = new List<VertexCandidate>();
            }

            return result;
        }

        private VertexCandidate? SelectUniqueVertexCandidate(List<VertexCandidate> candidates) {
            if (candidates.Count == 0) {
                return null;
            }

            candidates.Sort(static (left, right) => {
                var distanceComparison = left.Distance.CompareTo(right.Distance);
                return distanceComparison != 0
                    ? distanceComparison
                    : left.TargetIndex.CompareTo(right.TargetIndex);
            });

            if (candidates.Count > 1 &&
                candidates[1].Distance - candidates[0].Distance <=
                CandidateAmbiguityGridFactor * _tolerance + _numericSlack) {
                // A vertex that is almost equally close to two targets is common in
                // narrow corridors and near opposite sides of a thin polygon. Refusing
                // the anchor is safer than guessing which boundary owns the vertex.
                return null;
            }

            return candidates[0];
        }

        private void AddVertexToSegmentAnchors(
            RingModel source,
            RingModel target,
            IReadOnlyList<bool> sourceHasVertexAnchor,
            bool sourceIsFirst,
            ICollection<Anchor> anchors) {
            var anchorTolerance = AnchorGridFactor * _tolerance + _numericSlack;

            for (var sourceVertexIndex = 0;
                 sourceVertexIndex < source.VertexCount;
                 sourceVertexIndex++) {
                if (sourceHasVertexAnchor[sourceVertexIndex]) {
                    continue;
                }

                var candidates = new List<SegmentCandidate>();
                var sourceCoordinate = source.Vertices[sourceVertexIndex];

                for (var targetSegmentIndex = 0;
                     targetSegmentIndex < target.VertexCount;
                     targetSegmentIndex++) {
                    var targetLocation = target.Project(sourceCoordinate, targetSegmentIndex);
                    var targetSegmentLength = target.SegmentLengths[targetSegmentIndex];
                    var distanceFromNearestEndpoint = Math.Min(
                        targetLocation.Fraction * targetSegmentLength,
                        (1.0 - targetLocation.Fraction) * targetSegmentLength);

                    if (targetLocation.Distance > anchorTolerance ||
                        distanceFromNearestEndpoint <= _tolerance + _numericSlack ||
                        !VertexAndSegmentDirectionsAreCompatible(
                            source,
                            sourceVertexIndex,
                            target,
                            targetSegmentIndex)) {
                        continue;
                    }

                    candidates.Add(new SegmentCandidate(
                        targetLocation.Distance,
                        targetSegmentIndex,
                        targetLocation));
                }

                if (candidates.Count == 0) {
                    continue;
                }

                candidates.Sort(static (left, right) => {
                    var distanceComparison = left.Distance.CompareTo(right.Distance);
                    if (distanceComparison != 0) {
                        return distanceComparison;
                    }

                    var segmentComparison = left.SegmentIndex.CompareTo(right.SegmentIndex);
                    return segmentComparison != 0
                        ? segmentComparison
                        : left.TargetLocation.Fraction.CompareTo(right.TargetLocation.Fraction);
                });

                if (candidates.Count > 1 &&
                    candidates[1].Distance - candidates[0].Distance <=
                    CandidateAmbiguityGridFactor * _tolerance + _numericSlack) {
                    continue;
                }

                var candidate = candidates[0];
                var sourceLocation = source.VertexLocation(sourceVertexIndex);

                anchors.Add(sourceIsFirst
                    ? new Anchor(
                        sourceLocation,
                        candidate.TargetLocation,
                        candidate.Distance,
                        AnchorKind.FirstVertexToSecondSegment)
                    : new Anchor(
                        candidate.TargetLocation,
                        sourceLocation,
                        candidate.Distance,
                        AnchorKind.SecondVertexToFirstSegment));
            }
        }

        private List<Anchor> DeduplicateAnchors(
            RingModel first,
            RingModel second,
            List<Anchor> anchors) {
            anchors.Sort(static (left, right) => {
                var firstMeasureComparison = left.First.Measure.CompareTo(right.First.Measure);
                if (firstMeasureComparison != 0) {
                    return firstMeasureComparison;
                }

                var secondMeasureComparison = left.Second.Measure.CompareTo(right.Second.Measure);
                return secondMeasureComparison != 0
                    ? secondMeasureComparison
                    : left.Distance.CompareTo(right.Distance);
            });

            var result = new List<Anchor>(anchors.Count);

            foreach (var candidate in anchors) {
                var duplicateIndex = -1;

                for (var index = 0; index < result.Count; index++) {
                    if (first.CircularMeasureDistance(
                            candidate.First.Measure,
                            result[index].First.Measure) <= _tolerance + _numericSlack &&
                        second.CircularMeasureDistance(
                            candidate.Second.Measure,
                            result[index].Second.Measure) <= _tolerance + _numericSlack) {
                        duplicateIndex = index;
                        break;
                    }
                }

                if (duplicateIndex < 0) {
                    result.Add(candidate);
                    continue;
                }

                if (CompareAnchorPreference(candidate, result[duplicateIndex]) < 0) {
                    result[duplicateIndex] = candidate;
                }
            }

            result.Sort(static (left, right) =>
                left.First.Measure.CompareTo(right.First.Measure));
            return result;
        }

        private int CompareAnchorPreference(Anchor left, Anchor right) {
            if (left.Distance < right.Distance - _numericSlack) {
                return -1;
            }

            if (right.Distance < left.Distance - _numericSlack) {
                return 1;
            }

            if (left.Kind != right.Kind) {
                if (left.Kind == AnchorKind.VertexVertex) {
                    return -1;
                }

                if (right.Kind == AnchorKind.VertexVertex) {
                    return 1;
                }

                // Both candidates use one existing vertex and one projected position.
                // Prefer the lexicographically smaller existing vertex so duplicate
                // anchor resolution is independent of which ring was passed first.
                var existingVertexComparison = CompareCoordinates(
                    ExistingVertex(left),
                    ExistingVertex(right));
                if (existingVertexComparison != 0) {
                    return existingVertexComparison;
                }
            }

            var leftKey = CreateAnchorKey(left);
            var rightKey = CreateAnchorKey(right);
            return CompareCoordinateSequences(leftKey, rightKey);
        }

        private static Coordinate ExistingVertex(Anchor anchor) {
            return anchor.Kind switch {
                AnchorKind.VertexVertex =>
                    CompareCoordinates(anchor.First.Coordinate, anchor.Second.Coordinate) <= 0
                        ? anchor.First.Coordinate
                        : anchor.Second.Coordinate,
                AnchorKind.FirstVertexToSecondSegment => anchor.First.Coordinate,
                AnchorKind.SecondVertexToFirstSegment => anchor.Second.Coordinate,
                _ => throw new ArgumentOutOfRangeException(nameof(anchor)),
            };
        }

        private Coordinate[] CreateAnchorKey(Anchor anchor) {
            var first = MakePrecise(anchor.First.Coordinate);
            var second = MakePrecise(anchor.Second.Coordinate);
            return CompareCoordinates(first, second) <= 0
                ? new[] { first, second }
                : new[] { second, first };
        }
        #endregion

        #region LinearRingAlignmentOperation.Paths
        private List<CandidateInterval> BuildCandidateIntervals(
            RingModel first,
            RingModel second,
            IReadOnlyList<Anchor> anchors) {
            var result = new List<CandidateInterval>();

            for (var index = 0; index < anchors.Count; index++) {
                var start = anchors[index];
                var end = anchors[(index + 1) % anchors.Count];
                var firstPath = first.Slice(start.First, end.First);

                if (firstPath.Count < 2) {
                    continue;
                }

                PathEvidence? forwardEvidence = null;
                PathEvidence? reverseEvidence = null;
                List<Coordinate>? forwardPath = null;
                List<Coordinate>? reversePath = null;

                if (!ContainsIntermediateAnchor(
                        second,
                        anchors,
                        start,
                        end,
                        start.Second,
                        end.Second)) {
                    forwardPath = second.Slice(start.Second, end.Second);
                    forwardEvidence = EvaluatePathEvidence(
                        firstPath,
                        forwardPath,
                        requireMinimumLength: false);
                }

                if (!ContainsIntermediateAnchor(
                        second,
                        anchors,
                        start,
                        end,
                        end.Second,
                        start.Second)) {
                    reversePath = ReversePath(second.Slice(end.Second, start.Second));
                    reverseEvidence = EvaluatePathEvidence(
                        firstPath,
                        reversePath,
                        requireMinimumLength: false);
                }

                var forwardMatches = forwardEvidence?.IsMatch == true;
                var reverseMatches = reverseEvidence?.IsMatch == true;

                if (forwardMatches && !reverseMatches) {
                    result.Add(new CandidateInterval(
                        index,
                        start,
                        end,
                        +1,
                        forwardEvidence!));
                    continue;
                }

                if (reverseMatches && !forwardMatches) {
                    result.Add(new CandidateInterval(
                        index,
                        start,
                        end,
                        -1,
                        reverseEvidence!));
                    continue;
                }

                if (!forwardMatches || !reverseMatches) {
                    continue;
                }

                var preferredOrientation = SelectClearlyBetterOrientation(
                    forwardEvidence!,
                    reverseEvidence!);

                if (preferredOrientation > 0) {
                    result.Add(new CandidateInterval(
                        index,
                        start,
                        end,
                        +1,
                        forwardEvidence!));
                }
                else if (preferredOrientation < 0) {
                    result.Add(new CandidateInterval(
                        index,
                        start,
                        end,
                        -1,
                        reverseEvidence!));
                }
                // If both directions fit almost equally well, the geometry is locally
                // ambiguous (often a narrow loop). Do not align either direction.
            }

            return result;
        }

        private bool ContainsIntermediateAnchor(
            RingModel ring,
            IReadOnlyList<Anchor> anchors,
            Anchor startAnchor,
            Anchor endAnchor,
            Location arcStart,
            Location arcEnd) {
            foreach (var anchor in anchors) {
                if (ReferenceEquals(anchor, startAnchor) || ReferenceEquals(anchor, endAnchor)) {
                    continue;
                }

                if (ring.IsStrictlyBetweenForward(
                    arcStart.Measure,
                    arcEnd.Measure,
                    anchor.Second.Measure)) {
                    return true;
                }
            }

            return false;
        }

        private int SelectClearlyBetterOrientation(
            PathEvidence forward,
            PathEvidence reverse) {
            var threshold = CompetingPathScoreGridFactor * _tolerance + _numericSlack;

            if (Math.Abs(forward.MaximumDistance - reverse.MaximumDistance) > threshold) {
                return CompareEvidence(forward, reverse) <= 0 ? +1 : -1;
            }

            if (Math.Abs(forward.MeanDistance - reverse.MeanDistance) > threshold) {
                return CompareEvidence(forward, reverse) <= 0 ? +1 : -1;
            }

            return 0;
        }

        private static int CompareEvidence(PathEvidence left, PathEvidence right) {
            var maximumComparison = left.MaximumDistance.CompareTo(right.MaximumDistance);
            if (maximumComparison != 0) {
                return maximumComparison;
            }

            var meanComparison = left.MeanDistance.CompareTo(right.MeanDistance);
            if (meanComparison != 0) {
                return meanComparison;
            }

            return left.LengthDifference.CompareTo(right.LengthDifference);
        }

        private List<SharedRun> BuildSharedRuns(
            RingModel first,
            RingModel second,
            IReadOnlyList<Anchor> anchors,
            IReadOnlyList<CandidateInterval> intervals) {
            var acceptedByIndex = intervals.ToDictionary(
                static interval => interval.Index);
            var groups = GroupAcceptedIntervals(anchors.Count, acceptedByIndex);
            var result = new List<SharedRun>();

            foreach (var group in groups) {
                var participatingAnchors = new HashSet<Anchor>();
                foreach (var interval in group) {
                    participatingAnchors.Add(interval.Start);
                    participatingAnchors.Add(interval.End);
                }

                // Two vertex-to-vertex anchors provide evidence that the run has
                // corresponding topology positions at both ends (or around a full ring).
                // Purely staggered parallel overlaps usually provide only projection
                // anchors and are intentionally rejected as semantically ambiguous.
                if (participatingAnchors.Count(static anchor =>
                        anchor.Kind == AnchorKind.VertexVertex) < 2) {
                    continue;
                }

                var orientation = group[0].Orientation;
                var coversWholeRing = group.Count == anchors.Count;
                var start = group[0].Start;
                var end = group[^1].End;

                var firstPath = first.Slice(
                    start.First,
                    end.First,
                    coversWholeRing);

                var secondPath = orientation > 0
                    ? second.Slice(start.Second, end.Second, coversWholeRing)
                    : ReversePath(second.Slice(
                        end.Second,
                        start.Second,
                        coversWholeRing));

                var combinedEvidence = EvaluatePathEvidence(firstPath, secondPath);
                if (!combinedEvidence.IsMatch) {
                    continue;
                }

                var canonicalCoordinates = BuildCanonicalPath(
                    firstPath,
                    secondPath,
                    coversWholeRing);

                if (!CanonicalPathIsSafe(canonicalCoordinates, coversWholeRing)) {
                    continue;
                }

                result.Add(new SharedRun(
                    start,
                    end,
                    orientation,
                    canonicalCoordinates,
                    coversWholeRing));
            }

            return result;
        }

        private static List<List<CandidateInterval>> GroupAcceptedIntervals(
            int intervalCount,
            IReadOnlyDictionary<int, CandidateInterval> acceptedByIndex) {
            if (acceptedByIndex.Count == 0) {
                return new List<List<CandidateInterval>>();
            }

            var startIndex = 0;
            var foundBoundary = false;

            for (var index = 0; index < intervalCount; index++) {
                var previousIndex = (index - 1 + intervalCount) % intervalCount;
                var currentAccepted = acceptedByIndex.TryGetValue(index, out var current);
                var previousAccepted = acceptedByIndex.TryGetValue(previousIndex, out var previous);

                if (!currentAccepted ||
                    !previousAccepted ||
                    current!.Orientation != previous!.Orientation) {
                    startIndex = index;
                    foundBoundary = true;
                    break;
                }
            }

            // Every interval is accepted with one consistent orientation.
            if (!foundBoundary) {
                return new List<List<CandidateInterval>>
                {
                    Enumerable.Range(0, intervalCount)
                        .Select(index => acceptedByIndex[index])
                        .ToList(),
                };
            }

            var result = new List<List<CandidateInterval>>();
            List<CandidateInterval>? currentGroup = null;

            for (var offset = 0; offset < intervalCount; offset++) {
                var index = (startIndex + offset) % intervalCount;
                if (!acceptedByIndex.TryGetValue(index, out var interval)) {
                    if (currentGroup is not null) {
                        result.Add(currentGroup);
                        currentGroup = null;
                    }

                    continue;
                }

                if (currentGroup is not null &&
                    currentGroup[^1].Orientation != interval.Orientation) {
                    result.Add(currentGroup);
                    currentGroup = null;
                }

                currentGroup ??= new List<CandidateInterval>();
                currentGroup.Add(interval);
            }

            if (currentGroup is not null) {
                result.Add(currentGroup);
            }

            return result;
        }

        private PathEvidence EvaluatePathEvidence(
            IReadOnlyList<Coordinate> firstPath,
            IReadOnlyList<Coordinate> secondPath,
            bool requireMinimumLength = true) {
            if (firstPath.Count < 2 || secondPath.Count < 2) {
                return PathEvidence.NoMatch;
            }

            var distances = new List<double>(
                2 * (firstPath.Count + secondPath.Count));

            AddDirectedSampleDistances(firstPath, secondPath, distances);
            AddDirectedSampleDistances(secondPath, firstPath, distances);

            var maximumDistance = distances.Count == 0 ? 0.0 : distances.Max();
            var meanDistance = distances.Count == 0 ? 0.0 : distances.Average();
            var firstLength = PathLength(firstPath);
            var secondLength = PathLength(secondPath);
            var lengthDifference = Math.Abs(firstLength - secondLength);
            var lengthTolerance = Math.Max(
                4.0 * _tolerance,
                RelativeLengthTolerance * Math.Max(firstLength, secondLength));

            var directionMatches =
                DirectedPathDirectionsMatch(firstPath, secondPath) &&
                DirectedPathDirectionsMatch(secondPath, firstPath);

            var lengthIsSufficient =
                !requireMinimumLength ||
                Math.Min(firstLength, secondLength) >=
                    MinimumEvidenceLengthGridFactor * _tolerance - _numericSlack;

            var isMatch =
                lengthIsSufficient &&
                maximumDistance <= _pathEvidenceTolerance + _numericSlack &&
                lengthDifference <= lengthTolerance + _numericSlack &&
                directionMatches;

            return new PathEvidence(
                isMatch,
                maximumDistance,
                meanDistance,
                firstLength,
                secondLength,
                lengthDifference);
        }

        private static void AddDirectedSampleDistances(
            IReadOnlyList<Coordinate> source,
            IReadOnlyList<Coordinate> target,
            ICollection<double> distances) {
            if (source.Count == 0) {
                return;
            }

            distances.Add(DistanceToPath(source[0], target));

            for (var index = 0; index < source.Count - 1; index++) {
                var start = source[index];
                var end = source[index + 1];

                // Quarter-point sampling catches a short bow or stagger that endpoint-only
                // and midpoint-only checks can miss, while keeping the pairwise operation
                // predictable and inexpensive for normal polygon rings.
                for (var sampleIndex = 1; sampleIndex <= 3; sampleIndex++) {
                    var fraction = sampleIndex / 4.0;
                    var sample = new Coordinate(
                        start.X + fraction * (end.X - start.X),
                        start.Y + fraction * (end.Y - start.Y));
                    distances.Add(DistanceToPath(sample, target));
                }

                distances.Add(DistanceToPath(end, target));
            }
        }

        private bool DirectedPathDirectionsMatch(
            IReadOnlyList<Coordinate> source,
            IReadOnlyList<Coordinate> target) {
            var minimumSegmentLength =
                SignificantDirectionSegmentGridFactor * _tolerance;

            for (var sourceIndex = 0;
                 sourceIndex < source.Count - 1;
                 sourceIndex++) {
                var sourceStart = source[sourceIndex];
                var sourceEnd = source[sourceIndex + 1];
                var sourceLength = sourceStart.Distance(sourceEnd);

                if (sourceLength <= minimumSegmentLength + _numericSlack) {
                    continue;
                }

                var midpoint = new Coordinate(
                    (sourceStart.X + sourceEnd.X) / 2.0,
                    (sourceStart.Y + sourceEnd.Y) / 2.0);

                // Start with the geometrically nearest non-zero segment. This allows a
                // legitimate boundary represented entirely by one-cell steps to match a
                // longer segment in the other ring. If that tiny segment has an incompatible
                // direction (for example a micro-artifact at a corner), also try the nearest
                // significant segment before rejecting the path.
                var targetSegmentIndex = FindNearestSignificantSegment(
                    midpoint,
                    target,
                    0.0);

                if (targetSegmentIndex < 0) {
                    return false;
                }

                if (SegmentDirectionsMatch(
                        sourceStart,
                        sourceEnd,
                        target[targetSegmentIndex],
                        target[targetSegmentIndex + 1])) {
                    continue;
                }

                var significantTargetSegmentIndex = FindNearestSignificantSegment(
                    midpoint,
                    target,
                    minimumSegmentLength);

                if (significantTargetSegmentIndex < 0 ||
                    !SegmentDirectionsMatch(
                        sourceStart,
                        sourceEnd,
                        target[significantTargetSegmentIndex],
                        target[significantTargetSegmentIndex + 1])) {
                    return false;
                }
            }

            return true;
        }

        private static bool SegmentDirectionsMatch(
            Coordinate firstStart,
            Coordinate firstEnd,
            Coordinate secondStart,
            Coordinate secondEnd) {
            return UndirectedCosine(
                firstEnd.X - firstStart.X,
                firstEnd.Y - firstStart.Y,
                secondEnd.X - secondStart.X,
                secondEnd.Y - secondStart.Y) >= PathDirectionCosine;
        }

        private static int FindNearestSignificantSegment(
            Coordinate point,
            IReadOnlyList<Coordinate> path,
            double minimumSegmentLength) {
            var bestIndex = -1;
            var bestDistance = double.PositiveInfinity;

            for (var index = 0; index < path.Count - 1; index++) {
                if (path[index].Distance(path[index + 1]) <= minimumSegmentLength) {
                    continue;
                }

                var projection = ProjectPointToSegment(
                    point,
                    path[index],
                    path[index + 1]);

                if (projection.Distance < bestDistance) {
                    bestDistance = projection.Distance;
                    bestIndex = index;
                }
            }

            return bestIndex;
        }

        private List<Coordinate> BuildCanonicalPath(
            IReadOnlyList<Coordinate> firstPath,
            IReadOnlyList<Coordinate> secondPath,
            bool isClosed) {
            var selection = SelectBasePath(firstPath, secondPath, isClosed);
            var basePath = selection.BasePath;
            var otherPath = selection.OtherPath;
            var entries = new List<PathVertex>(basePath.Count + otherPath.Count);

            var baseMeasures = PathMeasures(basePath);
            for (var index = 0; index < basePath.Count; index++) {
                entries.Add(new PathVertex(
                    baseMeasures[index],
                    true,
                    index,
                    MakePrecise(basePath[index])));
            }

            for (var index = 0; index < otherPath.Count; index++) {
                var projection = ProjectPointToPath(otherPath[index], basePath);
                if (projection.Distance > _pathEvidenceTolerance + _numericSlack) {
                    continue;
                }

                entries.Add(new PathVertex(
                    projection.Measure,
                    false,
                    index,
                    MakePrecise(projection.Coordinate)));
            }

            entries.Sort(static (left, right) => {
                var measureComparison = left.Measure.CompareTo(right.Measure);
                if (measureComparison != 0) {
                    return measureComparison;
                }

                if (left.IsBaseVertex != right.IsBaseVertex) {
                    return left.IsBaseVertex ? -1 : 1;
                }

                var indexComparison = left.SourceIndex.CompareTo(right.SourceIndex);
                if (indexComparison != 0) {
                    return indexComparison;
                }

                return CompareCoordinates(left.Coordinate, right.Coordinate);
            });

            var canonical = new List<Coordinate>(entries.Count);
            for (var index = 0; index < entries.Count;) {
                var groupStart = index;
                var groupEnd = index + 1;

                while (groupEnd < entries.Count &&
                       Math.Abs(entries[groupEnd].Measure - entries[groupStart].Measure) <=
                       _numericSlack) {
                    groupEnd++;
                }

                // The sort order prefers an existing base vertex over a newly projected
                // coordinate at the same path measure.
                AppendExact(canonical, entries[groupStart].Coordinate.Copy());
                index = groupEnd;
            }

            if (canonical.Count == 0) {
                return canonical;
            }

            if (isClosed) {
                while (canonical.Count > 1 && canonical[0].Equals2D(canonical[^1])) {
                    canonical.RemoveAt(canonical.Count - 1);
                }

                canonical.Add(canonical[0].Copy());
            }
            else {
                canonical[0] = MakePrecise(basePath[0]);
                canonical[^1] = MakePrecise(basePath[^1]);
                RemoveConsecutiveDuplicates(canonical);
            }

            return canonical;
        }

        private BasePathSelection SelectBasePath(
            IReadOnlyList<Coordinate> firstPath,
            IReadOnlyList<Coordinate> secondPath,
            bool isClosed) {
            // Cost(first as base) is the displacement required to project the second
            // path onto the first. The lower maximum displacement wins, then the lower
            // mean displacement. This keeps the path requiring the least modification.
            var firstBaseCost = DirectedPathCost(secondPath, firstPath);
            var secondBaseCost = DirectedPathCost(firstPath, secondPath);

            if (firstBaseCost.Maximum < secondBaseCost.Maximum - _numericSlack) {
                return new BasePathSelection(firstPath, secondPath);
            }

            if (secondBaseCost.Maximum < firstBaseCost.Maximum - _numericSlack) {
                return new BasePathSelection(secondPath, firstPath);
            }

            if (firstBaseCost.Mean < secondBaseCost.Mean - _numericSlack) {
                return new BasePathSelection(firstPath, secondPath);
            }

            if (secondBaseCost.Mean < firstBaseCost.Mean - _numericSlack) {
                return new BasePathSelection(secondPath, firstPath);
            }

            var firstVertexCount = EffectiveVertexCount(firstPath, isClosed);
            var secondVertexCount = EffectiveVertexCount(secondPath, isClosed);

            if (firstVertexCount > secondVertexCount) {
                return new BasePathSelection(firstPath, secondPath);
            }

            if (secondVertexCount > firstVertexCount) {
                return new BasePathSelection(secondPath, firstPath);
            }

            // Final tie-breaker is independent of parameter order, path orientation,
            // and (for a full ring) start coordinate.
            return CompareCanonicalPathKeys(firstPath, secondPath, isClosed) <= 0
                ? new BasePathSelection(firstPath, secondPath)
                : new BasePathSelection(secondPath, firstPath);
        }

        private static int EffectiveVertexCount(
            IReadOnlyList<Coordinate> path,
            bool isClosed) {
            return isClosed && path.Count > 1 && path[0].Equals2D(path[^1])
                ? path.Count - 1
                : path.Count;
        }

        private PathCost DirectedPathCost(
            IReadOnlyList<Coordinate> source,
            IReadOnlyList<Coordinate> target) {
            var distances = new List<double>();
            AddDirectedSampleDistances(source, target, distances);
            return distances.Count == 0
                ? new PathCost(0.0, 0.0)
                : new PathCost(distances.Max(), distances.Average());
        }

        private int CompareCanonicalPathKeys(
            IReadOnlyList<Coordinate> first,
            IReadOnlyList<Coordinate> second,
            bool isClosed) {
            var firstKey = CreateCanonicalPathKey(first, isClosed);
            var secondKey = CreateCanonicalPathKey(second, isClosed);
            return CompareCoordinateSequences(firstKey, secondKey);
        }

        private Coordinate[] CreateCanonicalPathKey(
            IReadOnlyList<Coordinate> path,
            bool isClosed) {
            var precise = path.Select(MakePrecise).ToList();

            if (!isClosed) {
                var forward = precise.ToArray();
                var reverse = precise.AsEnumerable().Reverse().ToArray();
                return CompareCoordinateSequences(forward, reverse) <= 0
                    ? forward
                    : reverse;
            }

            if (precise.Count > 1 && precise[0].Equals2D(precise[^1])) {
                precise.RemoveAt(precise.Count - 1);
            }

            Coordinate[]? best = null;
            foreach (var direction in new[]
                     {
                         precise,
                         precise.AsEnumerable().Reverse().ToList(),
                     }) {
                for (var offset = 0; offset < direction.Count; offset++) {
                    var candidate = new Coordinate[direction.Count];
                    for (var index = 0; index < direction.Count; index++) {
                        candidate[index] = direction[(offset + index) % direction.Count];
                    }

                    if (best is null || CompareCoordinateSequences(candidate, best) < 0) {
                        best = candidate;
                    }
                }
            }

            return best ?? Array.Empty<Coordinate>();
        }

        private bool CanonicalPathIsSafe(
            IReadOnlyList<Coordinate> path,
            bool isClosed) {
            if (isClosed) {
                if (path.Count < 4 || !path[0].Equals2D(path[^1])) {
                    return false;
                }

                try {
                    var ring = _factory.CreateLinearRing(
                        path.Select(static coordinate => coordinate.Copy()).ToArray());
                    var polygon = _factory.CreatePolygon(ring);
                    return ring.IsClosed && ring.IsSimple && polygon.IsValid;
                }
                catch (ArgumentException) {
                    return false;
                }
            }

            if (path.Count < 2) {
                return false;
            }

            try {
                var line = _factory.CreateLineString(
                    path.Select(static coordinate => coordinate.Copy()).ToArray());
                return line.IsSimple;
            }
            catch (ArgumentException) {
                return false;
            }
        }
        #endregion

        #region LinearRingAlignmentOperation.Rebuild
        private LinearRing RebuildRing(
            RingModel model,
            IReadOnlyList<Replacement> replacements,
            string name) {
            if (replacements.Count == 0) {
                return model.Ring;
            }

            var wholeRingReplacements = replacements
                .Where(static replacement => replacement.CoversWholeRing)
                .ToList();

            if (wholeRingReplacements.Count > 0) {
                if (replacements.Count != 1 || wholeRingReplacements.Count != 1) {
                    throw new RingAlignmentException(
                        $"The {name} ring produced overlapping full-ring replacements.");
                }

                return CreateValidatedRing(
                    wholeRingReplacements[0].Coordinates,
                    name);
            }

            var origin = replacements
                .OrderBy(static replacement => replacement.Start.Measure)
                .First()
                .Start;

            var ordered = replacements
                .OrderBy(replacement =>
                    model.ForwardDistance(origin.Measure, replacement.Start.Measure))
                .ToList();

            ValidateReplacementOrder(model, ordered, origin, name);

            var coordinates = new List<Coordinate>();
            var current = origin;

            foreach (var replacement in ordered) {
                AppendOriginalArcInterior(
                    coordinates,
                    model.Slice(current, replacement.Start));
                AppendPath(coordinates, replacement.Coordinates);
                current = replacement.End;
            }

            AppendOriginalArcInterior(
                coordinates,
                model.Slice(current, origin));

            return CreateValidatedRing(coordinates, name);
        }

        private void ValidateReplacementOrder(
            RingModel model,
            IReadOnlyList<Replacement> ordered,
            Location origin,
            string name) {
            var previousEnd = 0.0;

            foreach (var replacement in ordered) {
                var start = model.ForwardDistance(
                    origin.Measure,
                    replacement.Start.Measure);
                var length = model.ForwardDistance(
                    replacement.Start.Measure,
                    replacement.End.Measure);
                var end = start + length;

                if (length <= _numericSlack) {
                    throw new RingAlignmentException(
                        $"The {name} ring produced an empty boundary replacement.");
                }

                if (start < previousEnd - _numericSlack ||
                    end > model.Length + _numericSlack) {
                    throw new RingAlignmentException(
                        $"The {name} ring produced overlapping or ambiguous boundary replacements.");
                }

                previousEnd = end;
            }
        }

        private void AppendOriginalArcInterior(
            ICollection<Coordinate> output,
            IReadOnlyList<Coordinate> originalArc) {
            // Replacement endpoints supply the canonical topology positions. Keep only
            // the private arc's interior vertices so an old near-duplicate endpoint is
            // not reintroduced beside the canonical endpoint.
            for (var index = 1; index < originalArc.Count - 1; index++) {
                AppendExact(output, MakePrecise(originalArc[index]));
            }
        }

        private static void AppendPath(
            ICollection<Coordinate> output,
            IReadOnlyList<Coordinate> path) {
            foreach (var coordinate in path) {
                AppendExact(output, coordinate.Copy());
            }
        }

        private LinearRing CreateValidatedRing(
            IReadOnlyList<Coordinate> source,
            string name) {
            var coordinates = new List<Coordinate>(source.Count + 1);
            foreach (var coordinate in source) {
                AppendExact(coordinates, MakePrecise(coordinate));
            }

            while (coordinates.Count > 1 && coordinates[0].Equals2D(coordinates[^1])) {
                coordinates.RemoveAt(coordinates.Count - 1);
            }

            if (coordinates.Count < 3) {
                throw new RingAlignmentException(
                    $"Alignment would collapse the {name} ring below three distinct vertices.");
            }

            coordinates.Add(coordinates[0].Copy());
            var result = _factory.CreateLinearRing(coordinates.ToArray());
            ValidateOutput(result, $"aligned {name}");
            return result;
        }

        private void ValidateOutput(LinearRing ring, string name) {
            if (!ring.IsClosed ||
                ring.NumPoints < 4 ||
                !ring.Coordinates[0].Equals2D(ring.Coordinates[^1])) {
                throw new RingAlignmentException(
                    $"The {name} ring is not exactly closed after alignment.");
            }

            if (!ring.IsSimple) {
                throw new RingAlignmentException(
                    $"The {name} ring would self-intersect or self-touch after alignment.");
            }

            var polygon = _factory.CreatePolygon(ring);
            var validity = new IsValidOp(polygon);
            if (!validity.IsValid) {
                var detail = validity.ValidationError?.Message ?? "unknown validity error";
                throw new RingAlignmentException(
                    $"The {name} ring would produce an invalid polygon: {detail}.");
            }
        }
        #endregion

    }
}
