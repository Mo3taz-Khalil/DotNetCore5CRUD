using DotNetCore5CRUD.Models;
using DotNetCore5CRUD.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCore5CRUD.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDBContext _Context;

        public MoviesController(ApplicationDBContext context)
        {
            _Context = context; 
        }


        public async Task<IActionResult> Index()
        {
            var movies = await _Context.Movies.ToListAsync();
            return View(movies);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new MovieForViewModel
            {
                Genres = await _Context.Genres.OrderBy(x=>x.Name).ToListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieForViewModel model)
        {
            model.Genres = await _Context.Genres.OrderBy(x => x.Name).ToListAsync();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var files = Request.Form.Files;
            if (!files.Any())
            {
                ModelState.AddModelError("Poster", "Please select movie poster");
                return View(model);
            }

            var poster = files.FirstOrDefault();
            var allowedExtintions = new List<string> { ".jpg", ".png" };

            if(!allowedExtintions.Contains(Path.GetExtension(poster.FileName).ToLower()))
            {
                ModelState.AddModelError("Poster", "Only .png , .jpg Allowd");
                return View(model);
            }


            if(poster.Length > 1048576)
            {
                ModelState.AddModelError("Poster", "Poster can not be more than 1 MB!");
                return View(model);
            }

            using var DateSteam = new MemoryStream();

            await poster.CopyToAsync(DateSteam);

            var movie = new Movie
            {
                Title = model.Title,
                GenreId = model.GenreId,
                Rate = model.Rate,
                StoryLine = model.StoryLine,
                Year = model.Year,
                Poster = DateSteam.ToArray()
            };

             _Context.Movies.Add(movie);
            await _Context.SaveChangesAsync();

          return  RedirectToAction(nameof(Index));
        }


    }
}
