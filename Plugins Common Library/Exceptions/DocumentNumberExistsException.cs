using System;

namespace Plugins_CommonLibrary.Exceptions
{
    public class DocumentNumberExistsException : Exception
    {
        public string DocumentNumber { get; set; }
        public DocumentNumberExistsException(string documentNumber)
        {
            this.DocumentNumber = documentNumber;
        }
    }
}
