

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Borsdata.Api.Dal.Model
{
   
    public partial class KpisCalcUpdatedRespV1
    {
        public DateTime kpisCalcUpdated { get; set; }
    }

}
