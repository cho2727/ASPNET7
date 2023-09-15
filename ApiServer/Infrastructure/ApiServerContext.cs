using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public partial class ApiServerContext : DbContext
{
    protected readonly IConfiguration _configuration;

    public ApiServerContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected ApiServerContext(IConfiguration configuration, DbContextOptions options) : base(options)
    {
        _configuration = configuration;
    }
    public ApiServerContext(IConfiguration configuration, DbContextOptions<ApiServerContext> options)
        : base(options)
    {
        _configuration = configuration;
    }

    public virtual DbSet<Login> Logins { get; set; }
    public virtual DbSet<User> Users { get; set; }

    /// <summary>
    /// Do not use Unicode as the default type for string
    /// </summary>
    /// <param name="modelBuilder"></param>
    private void DisableUnicodeToString(ModelBuilder modelBuilder)
    {
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(
                       p => p.ClrType == typeof(string)    // Entity is a string
                    && p.GetColumnType() == null           // No column type is set
                ))
        {
            property.SetIsUnicode(false);
        }
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    /// <summary>
    /// database first 로 작업할 경우
    /// db 쪽 table 에 column 을 추가하거나 수정한 후
    /// scaffold 를 통해 모델을 생성하면 새로 모델이 생성되어
    /// 사용자가 추가한 몇가지 코드를 (OnModelCreating) 다시 복사후 옮겨줘야 한다.
    /// 이러한 문제를 해결하기 위해 OnModelCrateingPartial 에 사용자가 정의한
    /// 내용들을 처리 하도록 할 수 있다. 
    /// </summary>
    /// <param name="modelBuilder"></param>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        //DisableUnicodeToString(modelBuilder);
    }
}
