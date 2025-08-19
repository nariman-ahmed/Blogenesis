using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blogenesis.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Blogenesis.DTO;
using Blogenesis.Helpers.Constants;
using System.Security.Claims;

namespace Blogenesis.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        public AccountController(UserManager<UserModel> userManager, SignInManager<UserModel> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            //validate any model errors
            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            //find user by email
            var userdb = await _userManager.FindByEmailAsync(loginDto.Email);

            //check if user found by email (thus email is registered)
            if (userdb == null)
            {
                ModelState.AddModelError(string.Empty, "Email isn't registered");
                return View(loginDto);
            }

            var existingUserClaims = await _userManager.GetClaimsAsync(userdb);
            if (!existingUserClaims.Any(c => c.Type == CustomClaims.FullName))
                await _userManager.AddClaimAsync(userdb, new Claim(CustomClaims.FullName, userdb.FullName));

            //user registered
            var result = await _signInManager.PasswordSignInAsync(userdb.UserName, loginDto.Password, isPersistent: false, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Wrong password.");
            }

            //else: all successful
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            var doesExist = await _userManager.FindByEmailAsync(registerDto.Email);
            if (doesExist != null)  //email already exists
            {
                ModelState.AddModelError(string.Empty, "Email already exists.");
                return View(registerDto);
            }

            //email doesn't already exist, so create new user

            //first check for passwords match
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View(registerDto);
            }

            //if all good, just create and add the new user
            var newUser = new UserModel()
            {
                FullName = registerDto.FirstName + ' ' + registerDto.LastName,
                Email = registerDto.Email,
                UserName = registerDto.Email,
                NormalizedEmail = registerDto.Email.ToUpperInvariant(),
                NormalizedUserName = registerDto.Email.ToUpperInvariant()
            };

            //add to database
            var result = await _userManager.CreateAsync(newUser, registerDto.Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "User registration failed.");
                return View(registerDto);
            }

            //all successful, next step is to add role to user
            var roleResult = await _userManager.AddToRoleAsync(newUser, AppRoles.User);
            if (!roleResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to add user to role.");
                return View(registerDto);
            }

            //finally, add the users name to the claim and sign in the user
            await _userManager.AddClaimAsync(newUser, new Claim(CustomClaims.FullName, newUser.FullName));
            await _signInManager.SignInAsync(newUser, isPersistent: false);

            //redirect to home page
            return RedirectToAction("Index", "Home");
        }
        
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}