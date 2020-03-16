using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public class UserEditController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserEditController(UserManager<ApplicationUser> userManager, ILogger<ApplicationRoleController> logger, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }


        public async Task<ActionResult> UsersDataSource([FromBody]DataManagerRequest dataManager)
        {
            IEnumerable<UserListViewModel> data = _userManager.Users.Select(r => new UserListViewModel
            {
                Id = r.Id,
                UserName = r.UserName,
                Name = r.Surname + ", " + r.FirstName + " " + r.MiddleName,
                Surname = r.Surname,
                FirstName = r.FirstName,
                MiddleName = r.MiddleName,
                EmployeeNumber = r.EmployeeNumber
            }).ToList();

            DataOperations operation = new DataOperations();
            int count = data.Count();
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

            //get the locked out status and roles of only the users to be displayed on the current page 
            foreach (var item in data)
            {
                //get all user's roles
                IList<string> userRolesList = new List<string>();
                userRolesList = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(item.Id));

                item.LockoutEnabled = await _userManager.IsLockedOutAsync(await _userManager.FindByIdAsync(item.Id));
                item.RoleNames = String.Join(", ", userRolesList);

                //add all rolenames default to false
                item.UserRoles = _roleManager.Roles.Select(r => new UserRolesViewModel
                {
                    Id = r.Id,
                    RoleName = r.Name,
                    HasRole = false
                }).ToList();

                //populate HasRole boolean field in the view model if the user has the role because we want to display all roles
                //marking the ones which the user currently has
                foreach (var userrole in item.UserRoles)
                {
                    if (userRolesList.Contains(userrole.RoleName))
                    {
                        userrole.HasRole = true;
                    }
                }
            }
            return Json(new { result = data, count = count });
        }

        //user information update is separated from roles update as they are in two different dialogs
        public async Task<ActionResult> Update([FromBody]CRUDModel<UserListViewModel> viewModel)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(viewModel.Value.Id);

            if (user != null)
            {
                string oldUserInformation = string.Format("Username: {0}, Surname: {1}, Firstname: {2}, Middlename: {3}, Employeenumber: {4}  ", user.UserName, user.Surname, user.FirstName, user.MiddleName, user.EmployeeNumber);
                string newUserInformation = string.Format("Username: {0}, Surname: {1}, Firstname: {2}, Middlename: {3}, Employeenumber: {4}  ", viewModel.Value.UserName, viewModel.Value.Surname, viewModel.Value.FirstName, viewModel.Value.MiddleName, viewModel.Value.EmployeeNumber);
                string oldUser = user.UserName + user.Surname + user.FirstName + user.MiddleName + user.EmployeeNumber;
                string newUser = viewModel.Value.UserName + viewModel.Value.Surname + viewModel.Value.FirstName + viewModel.Value.MiddleName + viewModel.Value.EmployeeNumber;
                if (oldUser != newUser) //update user
                {
                    user.UserName = viewModel.Value.UserName;
                    user.Surname = viewModel.Value.Surname;
                    user.FirstName = viewModel.Value.FirstName;
                    user.MiddleName = viewModel.Value.MiddleName;
                    user.EmployeeNumber = viewModel.Value.EmployeeNumber;
                    IdentityResult userResult = await _userManager.UpdateAsync(user);

                    if (userResult.Succeeded)
                    {
                        _logger.LogWarning(LoggingEvents.UserConfiguration, LoggingErrorText.editUserSucceed, oldUserInformation, newUserInformation, _userManager.GetUserName(User));
                    }
                    else
                    {
                        _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.editUserFailed, oldUserInformation, newUserInformation, _userManager.GetUserName(User), GetDataErrors.GetErrors(userResult));
                    }
                }
                else //update roles
                {
                    //changes to roles are not tracked therefore every role has to be checked for updates
                    foreach (var role in viewModel.Value.UserRoles)
                    {
                        if (await _userManager.IsInRoleAsync(user, role.RoleName) & !role.HasRole)
                        {
                            IdentityResult roleResult = await _userManager.RemoveFromRoleAsync(user, role.RoleName);
                            if (!roleResult.Succeeded)
                            {
                                _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.removeRoleFailed, role.RoleName, newUserInformation, _userManager.GetUserName(User), GetDataErrors.GetErrors(roleResult));
                            }
                        }
                        else if (role.HasRole & !(await _userManager.IsInRoleAsync(user, role.RoleName)))
                        {
                            IdentityResult roleResult = await _userManager.AddToRoleAsync(user, role.RoleName);
                            if (!roleResult.Succeeded)
                            {
                                _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.addRoleFailed, role.RoleName, newUserInformation, _userManager.GetUserName(User), GetDataErrors.GetErrors(roleResult));
                            }
                        }
                    }
                }
            }
            return Json(viewModel.Value);
        }

        public async void Delete([FromBody]CRUDModel<UserListViewModel> viewModel)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(viewModel.Value.Id);
            if (user != null)
            {
                string oldUserInformation = string.Format("Username: {0}, Surname: {1}, Firstname: {2}, Middlename: {3}, Employeenumber: {4}  ", user.UserName, user.Surname, user.FirstName, user.MiddleName, user.EmployeeNumber);
                IdentityResult userResult = await _userManager.DeleteAsync(user);
                if (userResult.Succeeded)
                {
                    _logger.LogWarning(LoggingEvents.UserConfiguration, LoggingErrorText.deleteUserSucceed, oldUserInformation, _userManager.GetUserName(User));
                }
                else
                {
                    _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.deleteUserFailed, oldUserInformation, _userManager.GetUserName(User), GetDataErrors.GetErrors(userResult));
                }
            }
        }


        public IActionResult EditPartial([FromBody] CRUDModel<UserListViewModel> value)
        {
            return PartialView("_DialogEditPartial", value.Value);
        }

        public IActionResult EditRolesPartial([FromBody] CRUDModel<UserListViewModel> value)
        {
            return PartialView("_DialogEditRolesPartial", value.Value);
        }

        public async Task<ActionResult> ManageRole(string id)
        {
            UserRoleEditViewModel viewModel = new UserRoleEditViewModel();
            if (!String.IsNullOrEmpty(id))
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    viewModel.Id = user.Id;
                    viewModel.Name = user.Surname + ", " + user.UserName + " " + user.MiddleName;

                    //add all rolenames default to false
                    viewModel.UserRoles = _roleManager.Roles.Select(r => new UserRolesViewModel
                    {
                        Id = r.Id,
                        RoleName = r.Name,
                        HasRole = false
                    }).ToList();

                    //get all user's roles
                    IList<string> userRolesList = new List<string>();
                    userRolesList = await _userManager.GetRolesAsync(user);

                    //populate HasRole boolean field in the view model if the user has the role because we want to display all roles
                    //marking the ones which the user currently has
                    foreach (var userrole in viewModel.UserRoles)
                    {
                        if (userRolesList.Contains(userrole.RoleName))
                        {
                            userrole.HasRole = true;
                        }
                    }
                    return View("ManageRole", viewModel);

                }
                else
                {
                    _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.retrieveUserFailed, id, _userManager.GetUserName(User));
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManageRole(string id, UserRoleEditViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ApplicationUser user = await _userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        string userInformation = string.Format("Username: {0}, Surname: {1}, Firstname: {2}, Middlename: {3}, Employeenumber: {4}  ", user.UserName, user.Surname, user.FirstName, user.MiddleName, user.EmployeeNumber);

                        //changes to roles are not tracked therefore every role has to be checked for updates
                        foreach (var role in viewModel.UserRoles)
                        {
                            if (await _userManager.IsInRoleAsync(user, role.RoleName) & !role.HasRole)
                            {
                                IdentityResult roleResult = await _userManager.RemoveFromRoleAsync(user, role.RoleName);
                                if (!roleResult.Succeeded)
                                {
                                    _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.removeRoleFailed, role.RoleName, userInformation, _userManager.GetUserName(User), GetDataErrors.GetErrors(roleResult));
                                }
                            }
                            else if (role.HasRole & !(await _userManager.IsInRoleAsync(user, role.RoleName)))
                            {
                                IdentityResult roleResult = await _userManager.AddToRoleAsync(user, role.RoleName);
                                if (!roleResult.Succeeded)
                                {
                                    _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.addRoleFailed, role.RoleName, userInformation, _userManager.GetUserName(User), GetDataErrors.GetErrors(roleResult));
                                }
                            }
                        }
                        return RedirectToAction(nameof(Index));
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View(viewModel);

            }
            return View(viewModel);
        }


        public async Task<ActionResult> LockUser(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    string userInformation = string.Format("Username: {0}, Surname: {1}, Firstname: {2}, Middlename: {3}, Employeenumber: {4}  ", user.UserName, user.Surname, user.FirstName, user.MiddleName, user.EmployeeNumber);

                    bool isLockedOut = await _userManager.IsLockedOutAsync(user);
                    if (!isLockedOut)
                    {
                        IdentityResult userResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                        if (userResult.Succeeded)
                        {
                            IdentityResult lockoutResult = await _userManager.SetLockoutEnabledAsync(user, true);
                            if (lockoutResult.Succeeded)
                            {
                                _logger.LogWarning(LoggingEvents.UserConfiguration, LoggingErrorText.userLockSucceed, userInformation, _userManager.GetUserName(User));
                            }
                            else
                            {
                                _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.userLockPartialFailed, userInformation, _userManager.GetUserName(User), GetDataErrors.GetErrors(userResult));
                                return BadRequest(InformationMessages.userLockPartialFail);
                            }
                        }
                        else
                        {
                            _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.userLockFailed, userInformation, _userManager.GetUserName(User), GetDataErrors.GetErrors(userResult));
                            return BadRequest(InformationMessages.userLockFail);
                        }
                    }
                    else
                    {
                        return BadRequest(InformationMessages.userAlreadyLocked);
                    }
                }
            }
            return Ok();
        }

        public async Task<ActionResult> UnlockUser(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    string userInformation = string.Format("Username: {0}, Surname: {1}, Firstname: {2}, Middlename: {3}, Employeenumber: {4}  ", user.UserName, user.Surname, user.FirstName, user.MiddleName, user.EmployeeNumber);

                    bool isLockedOut = await _userManager.IsLockedOutAsync(user);
                    if (isLockedOut)
                    {
                        IdentityResult userResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(-1));
                        if (userResult.Succeeded)
                        {
                            IdentityResult resetResult = await _userManager.ResetAccessFailedCountAsync(user);
                            IdentityResult lockoutResult = await _userManager.SetLockoutEnabledAsync(user, false);
                            if (lockoutResult.Succeeded && resetResult.Succeeded)
                            {
                                _logger.LogWarning(LoggingEvents.UserConfiguration, LoggingErrorText.userUnlockSucceed, userInformation, _userManager.GetUserName(User));
                            }
                            else
                            {
                                _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.userUnlockPartialFailed, userInformation, _userManager.GetUserName(User), GetDataErrors.GetErrors(userResult));
                                return BadRequest(InformationMessages.userUnlockPartialFail);
                            }
                        }
                        else
                        {
                            _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.userLockFailed, userInformation, _userManager.GetUserName(User), GetDataErrors.GetErrors(userResult));
                            return BadRequest(InformationMessages.userUnlockFailed);
                        }
                    }
                    else
                    {
                        return BadRequest(InformationMessages.userNotLocked);
                    }
                }
            }
            return Ok();
        }

        public async Task<ActionResult> ResetPassword(string id)
        {
            UserPasswordResetViewModel viewModel = new UserPasswordResetViewModel();
            if (!String.IsNullOrEmpty(id))
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    //if user is locked out user has to be unlocked before resetting password
                    bool isLockedOut = await _userManager.IsLockedOutAsync(user);
                    if (isLockedOut)
                    {
                        return RedirectToAction("Index");
                    }
                    viewModel.Id = user.Id;
                    viewModel.UserName = user.UserName;
                    viewModel.Surname = user.Surname;
                    viewModel.FirstName = user.FirstName;
                    viewModel.MiddleName = user.MiddleName;
                    viewModel.Email = user.Email;
                    viewModel.EmployeeNumber = user.EmployeeNumber;
                    return View(viewModel);
                }
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(string id, UserPasswordResetViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ApplicationUser user = await _userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        string userInformation = string.Format("Username: {0}, Surname: {1}, Firstname: {2}, Middlename: {3}, Employeenumber: {4}  ", user.UserName, user.Surname, user.FirstName, user.MiddleName, user.EmployeeNumber);
                        viewModel.UserName = user.UserName;
                        viewModel.Surname = user.Surname;
                        viewModel.FirstName = user.FirstName;
                        viewModel.MiddleName = user.MiddleName;
                        viewModel.Email = user.Email;
                        viewModel.EmployeeNumber = user.EmployeeNumber;

                        IdentityResult removeResult = await _userManager.RemovePasswordAsync(user);
                        if (removeResult.Succeeded)
                        {
                            IdentityResult addResult = await _userManager.AddPasswordAsync(user, viewModel.Password);
                            if (addResult.Succeeded)
                            {
                                _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.resetPasswordSucceeded, userInformation, _userManager.GetUserName(User));
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.resetPasswordFailed, userInformation, _userManager.GetUserName(User), GetDataErrors.GetErrors(addResult));
                                ModelState.AddModelError(string.Empty, "There was an error trying to reset the password");
                                ModelState.AddModelError(string.Empty, GetDataErrors.GetErrors(addResult));
                                return View(viewModel);
                            }
                        }
                        else
                        {
                            _logger.LogError(LoggingEvents.UserConfiguration, LoggingErrorText.removePasswordFailed, userInformation, _userManager.GetUserName(User), GetDataErrors.GetErrors(removeResult));
                            ModelState.AddModelError(string.Empty, "There was an error trying to reset the password");
                            ModelState.AddModelError(string.Empty, GetDataErrors.GetErrors(removeResult));
                            return View(viewModel);
                        }
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError(string.Empty, "There was an error");
                return View(viewModel);
            }
        }
    }

}