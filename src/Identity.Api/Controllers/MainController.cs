using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Controllers
{
    [ApiController]
    public abstract class MainController : Controller
    {
        protected ICollection<string> Errors = [];

        protected ActionResult CustomResponse(object? result = null)
        {
            if (IsValid())
            {
                return Ok(result);
            }

            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                {"Messages", Errors.ToArray() }
            }));
        }


        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            var errors = modelState.Values.SelectMany(e => e.Errors);
            foreach (var error in errors)
            {
                AddError(error.ErrorMessage);
            }

            return CustomResponse();
        }

        //protected ActionResult CustomResponse(ValidationResult validationResult)
        //{
        //    foreach (var error in validationResult.Errors)
        //    {
        //        AddError(error.ErrorMessage);
        //    }

        //    return CustomResponse();
        //}

        //protected ActionResult CustomResponse(ResponseResult responseResult)
        //{
        //    ResponseHasErrors(responseResult);

        //    return CustomResponse();
        //}

        //protected bool ResponseHasErrors(ResponseResult responseResult)
        //{
        //    if (responseResult == null || !responseResult.Errors.Messages.Any()) return false;

        //    foreach (var errorMessage in responseResult.Errors.Messages)
        //    {
        //        AddError(errorMessage);
        //    }

        //    return true;
        //}

        protected bool IsValid() => Errors.Count == 0;
        protected void AddError(string error) => Errors.Add(error);
        protected void CleanErrors() => Errors.Clear();
    }
}
