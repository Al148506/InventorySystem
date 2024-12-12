using InventorySystem.Models;

namespace InventorySystem.Service
{
    public class AuditService
    {
        private readonly DbInventoryContext _context;

        public AuditService(DbInventoryContext context)
        {
            _context = context;
        }

        public void LogChange(string UserId, string TypeAction, string TableName, string OldValues, string NewValues, string AffectedColumns, string PrimaryKey)
        {
            var log = new ChangeLog
            {
              UserId = UserId,
              TypeAction = TypeAction,
              TableName = TableName,
              OldValues = OldValues,
              NewValues = NewValues,
              AffectedColumns = AffectedColumns,
              PrimaryKey = PrimaryKey
            };

            _context.ChangeLogs.Add(log);
            _context.SaveChanges();
        }
    }
}
