// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using GCTWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace GCTWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _sender;

        public RegisterConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }

        public string Email { get; set; }
        public string StatusMessage { get; set; }
        public bool CanResend { get; set; }

        private const int CooldownSeconds = 30;

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;

            // Kiểm tra cooldown
            if (TempData["LastResendTime"] is DateTime lastResend)
            {
                var elapsed = DateTime.UtcNow - lastResend;
                CanResend = elapsed.TotalSeconds >= CooldownSeconds;
            }
            else
            {
                CanResend = true;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostResendEmailAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            // Kiểm tra cooldown
            if (TempData["LastResendTime"] is DateTime lastResend)
            {
                var elapsed = DateTime.UtcNow - lastResend;
                if (elapsed.TotalSeconds < CooldownSeconds)
                {
                    TempData["StatusMessage"] = $"Please wait {CooldownSeconds - (int)elapsed.TotalSeconds} seconds before resending.";
                    return RedirectToPage(new { email });
                }
            }

            // Generate confirmation token
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                protocol: Request.Scheme);

            // Gửi mail ở background thread, không block request
            Task.Run(() => _sender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>."));

            TempData["LastResendTime"] = DateTime.UtcNow;
            TempData["StatusMessage"] = "Confirmation email has been resent. Please check your inbox.";

            return RedirectToPage(new { email });
        }
    }
}
