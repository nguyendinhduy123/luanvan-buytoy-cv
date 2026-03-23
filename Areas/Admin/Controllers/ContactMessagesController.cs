using buytoy.Repository;
using Microsoft.AspNetCore.Mvc;

[Area("Admin")]
[Route("Admin/ContactMessages")]
public class ContactMessagesController : Controller
{
    private readonly DataContext _context;
    public ContactMessagesController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var messages = _context.ContactMessages
            .OrderByDescending(m => m.SentAt)
            .ToList();
        return View(messages);
    }
}
