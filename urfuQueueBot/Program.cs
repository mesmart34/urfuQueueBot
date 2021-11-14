using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace urfuQueueBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new TableParser("1mM1JgYBx188-fujNFJIfgWQPDm5QyvcSwFjsMCEDJzY");
            var rooms = parser.Parse();
            var links = new List<string>();
            foreach (var room in rooms)
            {
                Console.WriteLine(room.GetLink());

            }
            Console.ReadLine();

        }


    }
}
