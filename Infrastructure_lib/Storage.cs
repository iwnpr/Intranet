using Application_lib;
using Common_lib.Models.ServiceModels;
using Domain_lib;
using Domain_lib.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure_lib
{
    public class Storage(IConfiguration config, ILogger<Storage> logger) : IStorageService
    {
        private readonly ILogger<Storage> _logger = logger;
        private readonly string _storagePath = config.GetValue<string>("App:StoragePath") ?? "storage";

        public async Task<Result<TdFile>> SaveBlob(MemoryStream fileStream, TdFile file)
        {
            string error;
            try
            {
                var storage = Directory.CreateDirectory(_storagePath);
                file.FilePath = Path.Combine(storage.FullName, file.FileName);
                using FileStream fs = new(file.FilePath, FileMode.CreateNew);
                var buffer = fileStream.ToArray();
                await fs.WriteAsync(buffer.AsMemory(0, buffer.Length));
                return Result<TdFile>.Success(file);
            }
            catch(Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "{method}: Ошибка получения данных метод", StaticExtensions.GetCurrentMethodName());
                return Result<TdFile>.Error(-1, error);
            }
        }
    }
}
