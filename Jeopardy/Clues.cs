/*******************************************************************************
Coder:		Antonio Brito
Project:	Jeopardy
FileName:	Clues.cs

********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jeopardy
{
    public class Clues
    {
        public int id { get; set; }
        public string answer { get; set; }
        public string question { get; set; }
        public int? value { get; set; }
        public string airdate { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public int? category_id { get; set; }
    
    }
}
