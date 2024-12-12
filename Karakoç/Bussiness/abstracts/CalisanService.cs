using Karakoç.Models;

namespace Karakoç.Bussiness.abstracts
{
	public interface CalisanService
	{
		public List<Yevmiyeler> GetYevmiye(HttpContext httpContext);
	}
}
