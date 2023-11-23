/*******************************************************************************
Coder:		Antonio Brito
Project:	Jeopardy
FileName:	Category.cs

********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jeopardy
{
    public class Category
    {
        public int id { get; set; }
        public  string title { get; set; }
        public int? clues_count { get; set; }

        public List<Clues>clues = new List<Clues>();      
    }
}
