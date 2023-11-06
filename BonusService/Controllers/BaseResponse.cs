namespace BonusService.Controllers;

public class BaseResponse<T> : BaseResponseEmpty
{
    public BaseResponse(T? data)
    {
        Data = data;
    }
    public T? Data { get; set; }
}

public class BaseResponseEmpty
{
    public BaseResponseEmpty()
    {
        StatusCode = 0;
        StatusDescription = string.Empty;
    }
    public BaseResponseEmpty(int statusCode, string statusDescription)
    {
        StatusCode = statusCode;
        StatusDescription = statusDescription;
    }

    /// <summary>
    ///     Результат выполнения метода
    ///     0 - OK
    ///     1 - критическая ошибка
    ///     2 - штатная ошибка - полученный ответ не следует использовать
    /// </summary>
    public int StatusCode { get; set; }
    /// <summary>
    ///     Описание результата (отсутствует, если StatusCode 0)
    /// </summary>
    public string StatusDescription { get; set; }
}
