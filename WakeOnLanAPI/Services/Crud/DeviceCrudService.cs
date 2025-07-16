using DbData.Entities;
using Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Services.Crud.Abstracts;
using Services.Crud.Base;
using Services.DbTransactions.Abstracts;
using Services.User.Provider;

namespace Services.Crud;

public class DeviceCrudService: CrudService<Device>
{

    public DeviceCrudService(IUserProvider userSecurityMangerSrv): base(userSecurityMangerSrv)
    {
    }

    public override async Task<Device> CreateAsync(ITransactionScope scope, Device @object)
    {
        _userSecurityMangerSrv.GetUser().VerifyAdminRights(true);

        var dbEntity = await scope.GetRepository<Device>()
            .GetAll()
            .Where(d => d.Mac == @object.Mac)
            .FirstOrDefaultAsync();

        if (dbEntity is not null)
        {
            dbEntity.Name = @object.Name;
            dbEntity.Mac = @object.Mac;
            dbEntity.IpV4 = @object.IpV4;

            return Update(scope, dbEntity);
        }

        return await base.CreateAsync(scope, @object);
    }

    public override Device Update(ITransactionScope scope, Device @object)
    {
        return base.Update(scope, @object);
    }

    public override IQueryable<Device> GetAll(ITransactionScope scope)
    {
        var user = _userSecurityMangerSrv.GetUser();
        var query = base.GetAll(scope);
        
        if(!user.Admin) query = query.Where(e => e.AlowedUsers.Any(u => u.Id == user.UserId));

        return query;
    }
}