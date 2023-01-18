using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;


namespace PartRecorder
{
    public class Record 
    {
        public string Id { get; set; }
        public string PartNumber { get; set; }
        public int Quantity { get; set; }
    }
}
