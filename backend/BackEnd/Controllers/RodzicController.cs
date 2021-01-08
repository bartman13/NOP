﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.DataTransferObjects;
using BackEnd.Models;
using BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace BackEnd.Controllers
{
    /// <summary>
    /// Kontroler służący  do obsługi ządaużytkowników zalogowanych jako Rodzic
    /// </summary>
    [ApiController]
    [Authorize(Role.Rodzic)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class RodzicController : BaseController
    {
        private readonly NopContext _context;

        /// <summary>
        /// Konstruktor przyjmujacy kontekst bazy danych
        /// </summary>
        public RodzicController(NopContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Zwraca wszystkie zgłoszenia utworzone przez zalogowanego użytkownika
        /// </summary>
        /// <returns> Listę zgłoszeń </returns>
        [HttpGet("[controller]")]
        public IActionResult GetApps()
        {
            var zgl = (from Zgloszenia in _context.Zgloszenia.Include("Pacjent") // w przyszlosci zamienic na data transfer objet
                       where Zgloszenia.UzytId == Account.Id
                       select Zgloszenia).ToList();
            if (zgl == null) return BadRequest(new { message = "Lista uzytkownikow jest nullem" });
            return Ok(zgl);
        }
        /// <summary>
        /// Zwraca wszystkie szczepionki predefiniowane w systemie
        /// </summary>
        /// <returns> Listę szczepionek </returns>
        [HttpGet("Szczepionki")]
        public IActionResult GetVaccines()
        {
            var vaccines = _context.Szczepionki.ToList();
            if (vaccines == null) return BadRequest(new { message = "Lista szczepionek jest nullem" });
            return Ok(vaccines);
        }
        /// <summary>
        /// Zwraca wszystkie dzieci przypisane przez zalogowanego urzytkownika
        /// </summary>
        /// <returns> Listę dzieci </returns>
        [HttpGet("Dzieci")]
        public IActionResult GetChildren()
        {
            var children = _context.Pacjenci.Where(x => x.UzytId == Account.Id).ToList();
            if (children == null) return BadRequest(new { message = "Lista dzieci jest nullem" });
            return Ok(children);
        }
        /// <summary>
        /// Wyszukuje i zwraca dziecko o podanym id
        /// </summary>
        /// <param name="id"> Id dziecka </param>
        /// <returns> Pacjent o podanym id </returns>
        [HttpGet("Dziecko/{id}")]
        public IActionResult GetChild(int id)
        {
            var child = _context.Pacjenci.Where(x => x.Id == id).FirstOrDefault();
            if (child == null) return NotFound(new { message = "Pacjent o podanym id nie istnieje" });
            if (child.UzytId != Account.Id) return Unauthorized();
            return Ok(child);
        }
        /// <summary>
        /// Aktualizuje dane pacjenta o podanym id lub tworzy nowego.
        /// </summary>
        /// <param name="id"> Id pacjenta. </param>
        /// <param name="pacjent"> Zaktualizowane dane pacjenta. </param>
        /// <returns></returns>
        [HttpPost("Dziecko/{id?}")]
        public IActionResult UpdateChild(int? id, [FromBody] Pacjenci pacjent)
        {
            if(id == null)
            {
                _context.Pacjenci.Attach(new Pacjenci 
                    {
                        Imie = pacjent.Imie,
                        Nazwisko = pacjent.Nazwisko,
                        DataUrodzenia = pacjent.DataUrodzenia,
                        LekarzId = pacjent.LekarzId,
                        UzytId = Account.Id
                    }
                );
                _context.SaveChanges();
                return Ok();
            }
            var p = _context.Pacjenci.Where(p => p.Id == id).FirstOrDefault();
            if (p == null) return NotFound(new { message = "Pacjent o podanym id nie istnieje" });
            if (p.UzytId != Account.Id) return Unauthorized();
            p.Imie = pacjent.Imie;
            p.Nazwisko = pacjent.Nazwisko;
            p.DataUrodzenia = pacjent.DataUrodzenia;
            p.LekarzId = pacjent.LekarzId;
            _context.SaveChanges();
            return Ok();
        }
        /// <summary>
        /// Zwraca wszystkich lekarzy w systemie
        /// </summary>
        /// <returns> Lekarze </returns>
        [HttpGet("Lekarze")]
        public IActionResult GetDoctors()
        {
            var doctors = _context.Uzytkownicy.Where(u => u.Rola == 1).Select(u => new DoctorResponse { Id = u.Id, Imie = u.Imie, Nazwisko = u.Nazwisko });
            return Ok(doctors);
        }
        /// <summary>
        /// Zwraca predefiniowaną liste nieporządanych odczynów poszczepiennych dla danej szcczepionki
        /// </summary>
        /// <param name="id"> Id szczepionki </param>
        /// <returns> Listę niepożądanych odczynów </returns>
        [HttpGet("Nop/{id?}")]
        public IActionResult GetNop(int? id)
        {
            var nops = _context.Odczyny.Where(x => _context.SzczepionkiOdczyny
                .Where(y => y.SzczepionkaId == id).Any(z => z.OdczynId == x.Id))
                .Include("AtrybutyOdczynow").ToList();

            if (nops == null) return BadRequest(new { message = "Lista odczynow jest nullem" });
            return Ok(nops);
        }
        /// <summary>
        /// Tworzy nowe zgloszenie
        /// </summary>
        /// <param name="value"> Dane pozwalajace utworzyc nowe zgłoszenie </param>
        /// <returns> Listę Nop </returns>
        [HttpPost("UtworzZgloszenie")]
        public IActionResult CreateApp([FromForm] CreateAppRequest value)
        {
            if (value == null) return BadRequest(new { message = "Vallue jest nullem" });
            var user = _context.Uzytkownicy.Single(x => x.Id == Account.Id);
            DateTime timestamp = DateTime.Now;
            Zgloszenia app = new Zgloszenia
            {
                DataUtworzenia = timestamp,
                DataSzczepienia = value.Data,
                PacjentId = value.pacjentId,
                ProsbaOKontakt = value.prosba_o_kontakt,
                UzytId = Account.Id,
                ZdjecieKsZd = Guid.NewGuid().ToString() + Path.GetExtension(value.zdjecieKsZd.FileName)
            };
            _context.Zgloszenia.Attach(app);
            app.ZgloszenieSzczepionki.Add(new ZgloszenieSzczepionki { SzczepionkaId = value.szczepionkaId });
            foreach (var nop in value.nopy)
            {
                var oz = new OdczynyZgloszenia
                {
                    Data = timestamp,
                    DataWystapenia = nop.data,
                    OdczynId = nop.id
                };
                foreach (var attr in nop.atrybuty)
                {
                    oz.AtrybutyZgloszenia.Add(new AtrybutyZgloszenia
                    {
                        Wartosc = attr.wartosc,
                        AtodId = attr.id
                    });
                }
                app.OdczynyZgloszenia.Add(oz);
            }
            _context.SaveChanges();
            string directorypath = @"C:\NopImages";
            Directory.CreateDirectory(directorypath);
            using (FileStream stream = new FileStream(Path.Combine(directorypath, app.ZdjecieKsZd), FileMode.Create))
            {
                value.zdjecieKsZd.CopyTo(stream);
            }
            return Ok();
        }
        /// <summary>
        /// Zwraca listę id wszystkich szczepionek dotyczących zgłoszeniach o podanym id.
        /// </summary>
        /// <param name="id"> Id zgłoszenia </param>
        /// <returns> Lista id szczepionek </returns>
        [HttpGet("AppVaccines/{id}")]
        public IActionResult GetAppVaccines(int id)
        {
            var app = _context.Zgloszenia
                .Where(z => z.Id == id)
                .Include(i => i.ZgloszenieSzczepionki)
                .FirstOrDefault();
            if (app == null) return NotFound("Zgloszenie o podanym id nie istnieje");
            if (app.UzytId != Account.Id) return Unauthorized();
            List<int> vaccines = app.ZgloszenieSzczepionki.Select(zs => zs.SzczepionkaId).ToList();
            return Ok(vaccines);
        }
        /// <summary>
        /// Dodaje do zgłoszenia nowe podejrzewane niepożądane odczyny.
        /// </summary>
        /// <param name="id"> Id zgłoszenia </param>
        /// <param name="nops"> Lista nowych podejrzewanych niepożądanych odczynów </param>
        /// <returns> Ok </returns>
        [HttpPost("UpdateApp/{id?}")]
        public IActionResult UpdateApp(int? id, [FromBody] ICollection<NopAtrybuty> nops)
        {
            var app = _context.Zgloszenia
                   .Where(z => z.Id == id)
                   .Include(i => i.OdczynyZgloszenia)
                   .FirstOrDefault();
            if (app == null) return BadRequest("Zgloszenie o podanym id nie istnieje");
            if (app.UzytId != Account.Id) return Unauthorized();
            DateTime timestamp = DateTime.Now;
            foreach (var nop in nops)
            {
                var oz = new OdczynyZgloszenia
                {
                    Data = timestamp,
                    OdczynId = nop.id
                }; 
                foreach (var attr in nop.atrybuty)
                {
                    oz.AtrybutyZgloszenia.Add(new AtrybutyZgloszenia
                    {
                        Wartosc = attr.wartosc,
                        AtodId = attr.id
                    });
                }
                app.OdczynyZgloszenia.Add(oz);
            }
            _context.SaveChanges();
            return Ok();
        }
        /// <summary>
        /// Aktualizuje dane użytkownika.
        /// </summary>
        /// <param name="userUpdate"> Zaktualizowane dane użytkownika </param>
        /// <returns></returns>
        [HttpPost("UpdateUser")]
        public IActionResult UpdateUser([FromBody] UserUpdate userUpdate)
        {
            var user = _context.Uzytkownicy.Where(u => u.Id == Account.Id).FirstOrDefault();
            user.Imie = userUpdate.Imie;
            user.Nazwisko = userUpdate.Nazwisko;
            user.Email = userUpdate.Email;
            _context.SaveChanges();
            return Ok();
        }
        /// <summary>
        /// Przesyła plik do serwera i zapisuje go.
        /// </summary>
        /// <param name="file"> Dany plik </param>
        /// <returns></returns>
        [HttpPost("Upload")]
        public IActionResult UploadFile([FromForm] IFormFile file)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string directorypath = @"C:\NopImages";
            Directory.CreateDirectory(directorypath);
            using (FileStream stream = new FileStream(Path.Combine(directorypath, fileName), FileMode.Create))
            {
                file.CopyTo(stream);
            }
            _context.Pliki.Add(new Pliki
            {
                UzytId = Account.Id,
                OryginalnaNazwa = file.FileName,
                NazwaPliku = fileName
            });
            _context.SaveChanges();
            return Ok(fileName);
        }
        /// <summary>
        /// Usuwa plik z serwera
        /// </summary>
        /// <param name="filename"> Nazwa pliku </param>
        /// <returns></returns>
        [HttpDelete("File/{filename}")]
        public IActionResult DeleteFile(string filename)
        {
            var file = _context.Pliki.FirstOrDefault(f => f.NazwaPliku == filename);
            if (file == null) return NotFound();
            if (file.UzytId != Account.Id) return Unauthorized();
            string directorypath = @"C:\NopImages";
            FileInfo localFile = new FileInfo(Path.Combine(directorypath, filename));
            localFile.Delete();
            _context.Pliki.Remove(file);
            _context.SaveChanges();
            return Ok();
        }
    }
}
