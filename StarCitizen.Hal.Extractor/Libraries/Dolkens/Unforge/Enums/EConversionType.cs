using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarCitizen.Hal.Extractor.Library.Dolkens.Unforge.Enums
{
    public enum EConversionType : ushort
    {
        varAttribute = 0x00,
        varComplexArray = 0x01,
        varSimpleArray = 0x02,
        varClassArray = 0x03
    }
}
