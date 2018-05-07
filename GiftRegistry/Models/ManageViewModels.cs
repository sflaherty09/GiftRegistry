/**/
/*
    Name:

        ManageViewModel
    
    Purpose: 
        
        To handle all information needed to manage accout, like addding phone numbers
        or changing password
    
    Author:
        Sean Flaherty
 */
/**/
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;

namespace GiftRegistry.Models
{
    /**/
    /*
       Name
              IndexViewModel
           
       Purpose
              View all the important user information
           
       Author
              Automatically Generated
           
       Date
              1/30/2018
     */
    /**/
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    /**/
    /*
       Name
              ChangePasswordViewModel
           
       Purpose
              Shows user all the info they need to change their password, and assists in doing that
           
       Author
              Automatically Generated
           
       Date
              1/30/2018
     */
    /**/
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    /**/
    /*
       Name
              AddPhoneNumberViewModel
           
       Purpose
              Allows user to add phone number 
           
       Author
              Automatically Generated
           
       Date
              1/30/2018
     */
    /**/
    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    /**/
    /*
       Name
              VerifyPhoneNumberViewModel
           
       Purpose
              Verifies user's phone number
           
       Author
              Automatically Generated
           
       Date
              1/30/2018
     */
    /**/
    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    /**/
    /*
       Name
              ConfigureTwoFactorViewModel
           
       Purpose
              Allows user to set up two factor authentication
           
       Author
              Automatically Generated
           
       Date
              1/30/2018
     */
    /**/
    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }
}