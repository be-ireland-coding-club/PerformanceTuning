using DataContext;
using Microsoft.EntityFrameworkCore;

#region Example 4: N+1 problem
using (var db = new AdventureWorks2022Context())
{
    Console.WriteLine("Enter last name to search:");
    string strLastName = Console.ReadLine();

    var myPeople = db.People.Where(x => x.LastName == strLastName).ToList();

    foreach (var p in myPeople)
    {
        foreach(var e in p.EmailAddresses)
        {
            Console.WriteLine(e.EmailAddress1);
        }
    }

    Console.WriteLine("Hit any key to continue:");
    Console.ReadLine();
}
#endregion

#region Example 4: N+1 solution
using (var db = new AdventureWorks2022Context())
{
    Console.WriteLine("Enter last name to search:");
    string strLastName = Console.ReadLine();

    var myPeople = db.People.Where(x => x.LastName == strLastName).Include("EmailAddresses");

    foreach (var p in myPeople)
    {
        foreach (var e in p.EmailAddresses)
        {
            Console.WriteLine(e.EmailAddress1);
        }
    }

    Console.WriteLine("Hit any key to continue:");
    Console.ReadLine();
}
#endregion