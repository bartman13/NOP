﻿using BackEnd.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DataTransferObjects
{
    public class CreateAppRequest
    {
        public DateTime Data { get; set; }
        public int szczepionkaId { get; set; }
        public int pacjentId { get; set; }
        public bool prosba_o_kontakt { get; set; }
        public virtual ICollection<NopAtrybuty> nopy { get; set; }
        public virtual IFormFile zdjecieKsZd {get;set;}
        
    }
    
    public class NopAtrybuty
    {
        public int id { get; set; }
        public virtual ICollection<Atrybuty> atrybuty { get; set; }
       
        

    }
    public class Atrybuty
    {
       public int id { get; set; }
       public string wartosc { get; set; }
    }
}
