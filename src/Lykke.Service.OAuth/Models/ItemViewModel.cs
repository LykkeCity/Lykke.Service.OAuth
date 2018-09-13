using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.OAuth.Models
{
    public class ItemViewModel
    {
        public string Id { get; set; }

        public string Title { get; set; }
        public string Prefix { get; set; }

        public bool Selected { get; set; }
    }
}
