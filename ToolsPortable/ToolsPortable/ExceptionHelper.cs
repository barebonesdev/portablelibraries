using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public static class ExceptionHelper
    {
        public static Action<Exception> OnHandledExceptionOccurred;

        public static bool IsHResult(Exception ex, uint hresult)
        {
            return unchecked((uint)ex.HResult == hresult);
        }

        public static string GetFullDetail(Exception ex)
        {
            string thisException = string.Format("{0}: {1} - {2}", ex.GetType().Name, ex.Message, ex.StackTrace);

            if (ex.InnerException != null)
            {
                // Recursively get inner exception details
                string innerExceptionDetails = GetFullDetail(ex.InnerException);

                if (!string.IsNullOrEmpty(innerExceptionDetails))
                {
                    thisException = string.Format("{0}\n\nINNER: {1}", thisException, innerExceptionDetails);
                }
            }

            var aggregateException = ex as AggregateException;

            if (aggregateException != null && aggregateException.InnerExceptions != null)
            {
                int count = 1;

                foreach (var aggregateInner in aggregateException.InnerExceptions)
                {
                    // Don't include the aggregate that's just a duplicate of the inner.
                    if (aggregateInner != ex.InnerException)
                    {
                        // Recursively get aggregate inner exceptions
                        string aggregateInnerExceptionDetails = GetFullDetail(aggregateInner);
                        if (!string.IsNullOrEmpty(aggregateInnerExceptionDetails))
                        {
                            thisException = string.Format("{0}\n\nAGGREGATE({1}): {2}", thisException, count, aggregateInnerExceptionDetails);
                        }
                    }

                    count++;
                }
            }

            return thisException;
        }

        public static Exception GetInnerMostException(Exception ex)
        {
            if (ex.InnerException != null)
                return GetInnerMostException(ex.InnerException);

            return ex;
        }

        public static bool IsHttpWebIssue(Exception ex)
        {
            // Ignore HttpRequestException from telemetry since that just means offline
            // WebException means things like DNS name resolution error, connection timeout, network unreachable, etc
            // 0x80072EFF means some internal stream and file access stuff failed, not very common but nothing I can do
            return ex is System.Net.Http.HttpRequestException
                || ex is System.Net.WebException
                || ExceptionHelper.IsHResult(ex, 0x80072EFF);
        }

        public static void ReportHandledException(Exception ex)
        {
            OnHandledExceptionOccurred?.Invoke(ex);
        }
    }
}
