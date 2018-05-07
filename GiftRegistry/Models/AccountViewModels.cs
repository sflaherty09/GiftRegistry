/**/
/*
    Name:

        AccountViewModels
    
    Purpose: 
        
        To handle all information regarding a user's account. Holds log in and registration info,
        as well as two factor authentication etc. To be added to only be a controller class 
    
    Author:
        Sean Flaherty
 */
/**/
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace GiftRegistry.Models
{

    /**/
    /*
       Name
              SendCodeViewModel
           
       Purpose
              Holds info needed to send code to user's phone for two factor authentication
           
       Author
              Automatically Generated
           
       Date
              1/30/2018
     */
    /**/
    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    /**/
    /*
       Name
              VefifyCodeViewModel
           
       Purpose
              Vefify the code the user was sent over text message for two factor authentication 
           
       Author
              Automatically Generated
           
       Date
              1/30/2018
     */
    /**/
    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    /**/
    /*
       Name
              ForgotViewModel
           
       Purpose
              Takes user's email to verify they are a user, then helps them reset their password
           
       Author
              Automatically Generated
           
       Date
              1/30/2018
     */
    /**/
    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }


    /**/
    /*
       Name
              LoginViewModel
           
       Purpose
              Give the user the information they need to log in
           
       Author
              Automatically Generated
           
       Date
              1/30/2018
     */
    /**/
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }



    /**/
    /*
       Name
              RegisterViewModel
           
       Purpose
              Presents user with the information they need to register and account
           
       Author
              Automatically Generated
           
       Date
              1/30/2018
     */
    /**/
    public class RegisterViewModel
    {
        [RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$")]
        [Required]
        [StringLength(60)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

    }


    /**/
    /*
       Name
              ResetPasswordViewModel
           
       Purpose
              Contains the information necessary for user to reset their password
           
       Author
              Automatically Generated
           
       Date
              1/30/2018
     */
    /**/
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }


    /**/
    /*
       Name
              ForgotPasswordViewModel
           
       Purpose
              Takes user's email to verify they are a user, then helps them reset their password
           
       Author
              Automatically Generated
           
       Date
              1/30/2018
     */
    /**/
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } 
    }



    /**/
    /*
       Name
              GiftList
           
       Purpose
              Holds all the user's GiftList information, including name, 
              rating, cateogry, price, and link
           
       Author
              Sean Flaherty
           
       Date
              1/30/2018
     */
    /**/
    public class GiftList
    {
        public int ID { get; set; }
        [Display(Name ="Gift Name")]
        [StringLength(60)]
        [Required]
        public string GiftName { get; set; }
        [Range(1, 10)]
        [Required]
        public int Rating { get; set; }
        [RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$")]
        [Required]
        [StringLength(30)]
        public string Category { get; set; }
        [Range(1, int.MaxValue)]
        [DataType(DataType.Currency)]
        [Required]
        public decimal Price { get; set; }
        [DataType(DataType.Url)]
        [Required]
        public string Link { get; set; }

        public string UserId { get; set; }
        public bool Bought { get; set; }
    }
}
