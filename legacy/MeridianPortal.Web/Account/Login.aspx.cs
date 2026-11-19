using System;
using System.Web.Security;
using System.Web.UI;

namespace MeridianPortal.Web.Account
{
    public partial class Login : Page
    {
        protected void LoginButton_Click(object sender, EventArgs e)
        {
            if (Membership.ValidateUser(UserName.Text, Password.Text))
            {
                var profile = UserProfileRepository.Load(UserName.Text);
                Session["UserRegion"] = profile.Region;
                FormsAuthentication.RedirectFromLoginPage(UserName.Text, RememberMe.Checked);
            }
            else
            {
                ErrorMessage.Text = "Invalid user name or password.";
            }
        }
    }
}
