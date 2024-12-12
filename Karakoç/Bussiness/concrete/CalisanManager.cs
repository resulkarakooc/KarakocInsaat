using Karakoç.Bussiness.abstracts;
using Karakoç.Models;

namespace Karakoç.Bussiness.concrete
{
	public class CalisanManager : CalisanService
	{
		private readonly ResulContext _context;

		public CalisanManager(ResulContext context)
		{
			_context = context;
		}

		public class YevmiyeViewClass
		{
			public List<Yevmiyeler> YevmiyelerList { get; set; } // İsim değişikliği
		}

		public List<Yevmiyeler> GetYevmiye(HttpContext httpContext)
		{
			var calisanId = httpContext.Session.GetInt32("CalisanId");

			var yevmiyeler = _context.Yevmiyelers
				.Where(y => y.CalisanId == calisanId)
				.ToList();

			var yevmiye = new YevmiyeViewClass
			{
				YevmiyelerList = yevmiyeler // İsim değişikliği
			};

			// ViewBag ile view'a aktar
			

			return yevmiyeler;

		}
	}
}
