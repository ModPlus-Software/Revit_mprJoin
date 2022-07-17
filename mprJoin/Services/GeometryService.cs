namespace mprJoin.Services;

using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Models;

/// <summary>
/// Сервис по работе с геометрией
/// </summary>
public class GeometryService
{
    private const double RayDistance = 1000;
    private const double Tolerance = 0.00001;
    
    /// <summary>
    /// Проверяет являются ли две кривые типа Arc параллельными
    /// </summary>
    /// <param name="arcOne">Первая кривая</param>
    /// <param name="arcTwo">Вторая кривая</param>
    public bool IsArcParallel(Arc arcOne, Arc arcTwo)
    {
        var whoCurveInfo = GetArcInfo(arcOne);
        var withWhoCurveInfo = GetArcInfo(arcTwo);

        var whoCurveRays = GetRaysFromArcInfo(whoCurveInfo);
        var withWhoCurveRays = GetRaysFromArcInfo(withWhoCurveInfo);

        var oneArcIntersectDistance = GetIntersectionDistance(arcTwo, whoCurveRays);
        var twoArcIntersectDistance = GetIntersectionDistance(arcOne, withWhoCurveRays);

        if (oneArcIntersectDistance == 0 || twoArcIntersectDistance == 0)
            return false;

        return Math.Abs(oneArcIntersectDistance - twoArcIntersectDistance) < Tolerance;
    }
    
    private ArcInfo GetArcInfo(Arc arc)
    {
        var arcTangent = arc.ComputeDerivatives(0.5, false);

        return new ArcInfo
        {
            Tangent = arcTangent.Origin,
            PerpendicularDirection = arcTangent.BasisY,
        };
    }
    
    private List<Line> GetRaysFromArcInfo(ArcInfo arcInfo)
    {
        var rays = new List<Line>
        {
            Line.CreateBound(arcInfo.Tangent, arcInfo.Tangent + (arcInfo.PerpendicularDirection * RayDistance)),
            Line.CreateBound(arcInfo.Tangent, arcInfo.Tangent - (arcInfo.PerpendicularDirection * RayDistance))
        };

        return rays;
    }

    private double GetIntersectionDistance(Arc arc, List<Line> rays)
    {
        var oneArcIntersectDistance = 0d;
        foreach (var ray in rays)
        {
            var intersectResult = arc.Intersect(ray, out var intersectionResultArray);
            if (intersectResult != SetComparisonResult.Overlap || intersectionResultArray.IsEmpty)
                continue;

            oneArcIntersectDistance = intersectionResultArray.get_Item(0).XYZPoint.DistanceTo(ray.GetEndPoint(0));
            break;
        }

        return oneArcIntersectDistance;
    }
}