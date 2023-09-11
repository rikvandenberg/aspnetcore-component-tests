using System.ComponentModel.DataAnnotations;

public class RabbitMqSettings 
{
    [Required]
    public string HostName {get; init;} = default!;

    [Required]
    public string UserName {get; init;} = default!;

    [Required]
    [DataType(DataType.Password)]
    public string Password {get; init;} = default!;
}