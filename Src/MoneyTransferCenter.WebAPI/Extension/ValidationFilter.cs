using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MoneyTransferCenter.WebAPI.Extension;

//burası otomatik olarak çalışacak ve gelen requestin body kısmındaki parametreleri alıp, bu parametreler için DI'da kayıtlı olan validatorları çalıştıracak. Eğer validatorlar geçerli değilse, 400 BadRequest dönecek.
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {

        // ActionArguments içindeki tüm parametreleri dolaş
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null)
            {
                continue;
            }

            // Bu parametre tipi için DI'da kayıtlı bir validator var mı?
            Type validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            // DI'dan validator'ı al
            object? validator = context.HttpContext.RequestServices.GetService(validatorType);

            // Eğer validator yoksa, bir sonraki parametreye geç
            if (validator == null)
            {
                continue;
            }

            // Validator'ı çalıştır
            var validateMethod = validatorType.GetMethod("ValidateAsync",
                new[] { argument.GetType(), typeof(CancellationToken) });

            // Eğer ValidateAsync metodu bulunamazsa, bir sonraki parametreye geç
            ValidationResult validationResult = await(dynamic)validateMethod!
                .Invoke(validator, new[] { argument, CancellationToken.None })!;

            // Eğer doğrulama başarısızsa, 400 BadRequest döndür
            if (validationResult.IsValid == false)
            {
                var errors = validationResult.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Error = e.ErrorMessage
                });
                context.Result = new BadRequestObjectResult(new
                {
                    StatusCode = 400,
                    Message = "Doğrulama hatası oluştu.",
                    Errors = errors
                });
                return;
            }

        }
        await next();
    }
}
