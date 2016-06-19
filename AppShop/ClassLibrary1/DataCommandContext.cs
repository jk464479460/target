using System.Data.Entity;

namespace FrameWrok.Common
{
    public class DataCommandContext<TContext> where TContext :DbContext,new()
    {
        private TContext _context;

        public DataCommandContext()
        {
            _context=new TContext();
        } 
        public  TContext GetContext()
        {
            return _context;
        }
    }
}
