using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.ShipPartExpansion
{
    public interface IShipPartExpansionAPI
    {
        public void RenderPart(Part part, int localX, Ship ship, G g, Vec v, Vec worldPos);
        public IEnumerable<ICustomPart> GetCustomParts(Ship ship);
        public ICustomPart? GetCustomPart(Part part);
    }

    public interface ICustomPart
    {
        public void Render(Ship ship, int localX, G g, Vec v, Vec worldPos) { }
        public bool DoVanillaRender(Ship ship, int localX, G g) => true;
        public void RenderUI(Ship ship, G g, Combat? combat, int localX, string keyPrefix, bool isPreview, Vec v) { }
        public int RenderDepth => 0;
        public bool IsTemporary { get; set; }
    }
}
