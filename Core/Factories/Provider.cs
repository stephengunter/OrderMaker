using Core.Helpers;

namespace Core.Factories
{
	
	public class ProviderFactory
    {
        public static IOrderMaker Create(string name, string ip = "", string sid = "", string password = "")
        {
            if (name.EqualTo("Fake")) return new FakeOrderMaker(ip, sid, password);
            else if (name.EqualTo("HuaNan")) return new HuaNanDDSCOrderMaker(ip, sid, password);
            else return new ConcordOrderMaker(ip, sid, password);
        }
	}
}
