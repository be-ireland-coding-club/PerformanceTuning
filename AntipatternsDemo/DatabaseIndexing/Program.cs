using DataContext;

#region Example 1: Database Indexing problem
using (var db = new AdventureWorks2022Context())
{
    Console.WriteLine("Enter last name to search:");
    string strLastName = Console.ReadLine();

    var myPeople = db.Products.Where(x => x.)

    foreach (var p in myPeople)
    {
        if (p.LastName == strLastName)
        {
            Console.WriteLine(p.BusinessEntityId);
        }
    }

    Console.WriteLine("Hit any key to continue:");
    Console.ReadLine();
}
#endregion

#region Example 1: Database Indexing solution
using (var db = new AdventureWorks2022Context())
{
    Console.WriteLine("Enter last name to search:");
    string strLastName = Console.ReadLine();

    var myPeople = db.People.Where(x => x.LastName == strLastName);

    foreach (var p in myPeople)
    {
        Console.WriteLine(p.BusinessEntityId);
    }

    Console.WriteLine("Hit any key to continue:");
    Console.ReadLine();
}
#endregion