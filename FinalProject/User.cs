using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject
{
    public class User
    {
        [Key]
        public string userId { get; set; }
        public string speakerId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string phrase { get; set; }
        public bool access { get; set; }
    }
}
