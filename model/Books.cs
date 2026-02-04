using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentLibrary.model
{
    public class Books
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime PublishedDate { get; set; }
        public string Description { get; set; }
        public string CategoryId { get; set; }
        public string Category { get; set; }
        public int Copies { get; set; }
    }
}
