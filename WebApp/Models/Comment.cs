using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class Comment
    {
        public string Text { get; set; }

        public string Language { get; set; }

        public double Score { get; set; }

        public DateTime Date { get; set; }
    }
}