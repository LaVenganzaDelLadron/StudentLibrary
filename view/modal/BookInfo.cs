using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StudentLibrary.model;

namespace StudentLibrary.view.modal
{
    public partial class BookInfo : Form
    {
        public BookInfo()
        {
            InitializeComponent();
        }

        public BookInfo(Books book) : this()
        {
            SetBookData(book);
        }

        public void SetBookData(Books book)
        {
            lblTitle.Text = $"Title: {book.Title}";
            lblAuthor.Text = $"Author: {book.Author}";
            lblPublished.Text = $"Published Year: {book.PublishedDate.Year}";
            lblCategory.Text = $"Category: {book.Category}";
            lblDescription.Text = $"Description: {book.Description}";
        }
    }
}
