using Microsoft.AspNetCore.Http;

namespace TeachNote_Backend.DTOs
{
    public class NoteUploadDto
    {
        public IFormFile PdfFile { get; set; }
        public int SubjectId { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; }
    }
}
