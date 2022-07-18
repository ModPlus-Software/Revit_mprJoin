namespace mprJoin.Models;

using Autodesk.Revit.DB;

/// <summary>
/// Информация об кривых типа Arc
/// </summary>
public class ArcInfo
{  
    /// <summary>
    /// Касательная к центральной точке дуги
    /// </summary>
    public XYZ Tangent { get; set; }
    
    /// <summary>
    /// Перпендикулярное направление к касательной
    /// </summary>
    public XYZ PerpendicularDirection { get; set; }
}