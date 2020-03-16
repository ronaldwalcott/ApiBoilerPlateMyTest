using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using APiBoilerPlateWeb.Areas.UserManagement.Models;
using APiBoilerPlateWeb.Constants;
using APiBoilerPlateWeb.Data;
using APiBoilerPlateWeb.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Syncfusion.EJ2.Base;

namespace APiBoilerPlateWeb.Areas.UserManagement.Controllers
{
    [Area(AreaConfiguration.UserManagement)]
    public class ApplicationRoleController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationRoleController(UserManager<ApplicationUser> userManager, ILogger<ApplicationRoleController> logger, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult RolesDataSource([FromBody]DataManagerRequest dataManager)
        {
            IEnumerable data = _roleManager.Roles.Select(r => new ApplicationRolesViewModel
            {
                Id = r.Id,
                RoleName = r.Name
            }).ToList();
            DataOperations operation = new DataOperations();
            int count = data.Cast<ApplicationRolesViewModel>().Count();
            if (dataManager.Search != null && dataManager.Search.Count > 0) //Searching 
            {
                data = operation.PerformSearching(data, dataManager.Search);
            }

            if (dataManager.Sorted != null && dataManager.Sorted.Count > 0) //Sorting 
            {
                data = operation.PerformSorting(data, dataManager.Sorted);
            }

            if (dataManager.Skip != 0)                                      //Paging
            {
                data = operation.PerformSkip(data, dataManager.Skip);
            }
            if (dataManager.Take != 0)
            {
                data = operation.PerformTake(data, dataManager.Take);
            }
            return Json(new { result = data, count = count });
        }
        public async Task<ActionResult> Insert([FromBody]CRUDModel<ApplicationRolesViewModel> viewModel)
        {
            IdentityRole identityRole = new IdentityRole();
            identityRole.Name = viewModel.Value.RoleName;
            IdentityResult roleResult = await _roleManager.CreateAsync(identityRole);
            if (roleResult.Succeeded)
            {
                _logger.LogWarning(LoggingEvents.UserConfiguration, LoggingErrorText.newRoleSucceed, viewModel.Value.RoleName, _userManager.GetUserName(User));
            }
            else
            {
                _logger.LogWarning(LoggingEvents.UserConfiguration, LoggingErrorText.newRoleFailed, viewModel.Value.RoleName, _userManager.GetUserName(User), GetDataErrors.GetErrors(roleResult));
            }
            return Json(viewModel.Value);
        }
        public async Task<ActionResult> Update([FromBody]CRUDModel<ApplicationRolesViewModel> viewModel)
        {
            IdentityRole identityRole = await _roleManager.FindByIdAsync(viewModel.Value.Id);
            if (identityRole != null)
            {
                string oldIdentityRoleName = identityRole.Name;
                identityRole.Name = viewModel.Value.RoleName;
                IdentityResult roleResult = await _roleManager.UpdateAsync(identityRole);
                if (roleResult.Succeeded)
                {
                    _logger.LogWarning(LoggingEvents.UserConfiguration, LoggingErrorText.editRoleSucceed, oldIdentityRoleName, viewModel.Value.RoleName, _userManager.GetUserName(User));
                }
                else
                {
                    _logger.LogWarning(LoggingEvents.UserConfiguration, LoggingErrorText.editRoleFailed, oldIdentityRoleName, viewModel.Value.RoleName, _userManager.GetUserName(User), GetDataErrors.GetErrors(roleResult));
                }
            }
            return Json(viewModel.Value);
        }
        public async Task<ActionResult> Delete([FromBody]CRUDModel<ApplicationRolesViewModel> viewModel)
        {
            IdentityRole identityRole = await _roleManager.FindByIdAsync(viewModel.Key.ToString());
            if (identityRole != null)
            {
                string roleNameToBeDeleted = identityRole.Name;
                IdentityResult roleResult = await _roleManager.DeleteAsync(identityRole);
                if (roleResult.Succeeded)
                {
                    _logger.LogWarning(LoggingEvents.UserConfiguration, LoggingErrorText.deleteRoleSucceed, roleNameToBeDeleted, _userManager.GetUserName(User));
                }
                else
                {
                    _logger.LogWarning(LoggingEvents.UserConfiguration, LoggingErrorText.deleteRoleFailed, roleNameToBeDeleted, _userManager.GetUserName(User), GetDataErrors.GetErrors(roleResult));
                }
            }
            return Json(identityRole);
        }



        public async Task<ActionResult> ManageClaim(string id)
        {
            RoleClaimsViewModel viewModel = new RoleClaimsViewModel();
            if (!String.IsNullOrEmpty(id))
            {
                IdentityRole identityRole = await _roleManager.FindByIdAsync(id);
                if (identityRole != null)
                {
                    //Get claims associated with the role
                    IList<Claim> roleClaimList = await _roleManager.GetClaimsAsync(identityRole);
                    List<string> roleClaimTypeList = new List<string>();

                    foreach (var roleClaim in roleClaimList)
                    {
                        roleClaimTypeList.Add(roleClaim.Type);
                    }

                    viewModel.Id = identityRole.Id;
                    viewModel.RoleName = identityRole.Name;
                    viewModel.RoleClaims = new List<ClaimsViewModel>();

                    foreach (var claimName in ClaimNames.ClaimName)
                    {
                        viewModel.RoleClaims.Add(new ClaimsViewModel() { ClaimName = claimName, HasClaim = roleClaimTypeList.Contains(claimName) });
                    }

                    return View("ManageClaim", viewModel);
                }
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManageClaim(string id, RoleClaimsViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (!String.IsNullOrEmpty(id))
                {
                    IdentityRole identityRole = await _roleManager.FindByIdAsync(id);
                    if (identityRole != null)
                    {
                        //Get claims associated with the role
                        IList<Claim> roleClaimList = await _roleManager.GetClaimsAsync(identityRole);

                        //Extract the claim type
                        List<string> roleClaimTypeList = new List<string>();
                        foreach (var roleClaim in roleClaimList)
                        {
                            roleClaimTypeList.Add(roleClaim.Type);
                        }

                        foreach (var roleClaim in viewModel.RoleClaims)
                        {
                            //create a new claim with the claim name
                            Claim claim = new Claim(roleClaim.ClaimName, "");

                            //get the associated claim from the role's claim list
                            Claim associatedClaim = roleClaimList.Where(x => x.Type == roleClaim.ClaimName).FirstOrDefault();

                            if (roleClaim.HasClaim && !roleClaimTypeList.Contains(roleClaim.ClaimName))
                            {
                                IdentityResult claimResult = await _roleManager.AddClaimAsync(identityRole, claim);
                                if (!claimResult.Succeeded)
                                {
                                    _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.addClaimFailed, roleClaim.ClaimName, identityRole, _userManager.GetUserName(User), GetDataErrors.GetErrors(claimResult));
                                }
                            }
                            else if (!roleClaim.HasClaim && roleClaimTypeList.Contains(roleClaim.ClaimName))
                            {
                                IdentityResult claimResult = await _roleManager.RemoveClaimAsync(identityRole, associatedClaim);

                                if (!claimResult.Succeeded)
                                {
                                    _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.removeClaimFailed, roleClaim.ClaimName, identityRole, _userManager.GetUserName(User), GetDataErrors.GetErrors(claimResult));
                                }
                            }
                        }
                    }
                    return RedirectToAction("Index");
                }
            }

            return View(viewModel);
        }

        //private string GetErrors(IdentityResult result)
        //{
        //    string errorDescription = "";
        //    foreach (var error in result.Errors)
        //    {
        //        errorDescription = errorDescription + " " +  error.Description;
        //    }
        //    return errorDescription;
        //}
    }
}