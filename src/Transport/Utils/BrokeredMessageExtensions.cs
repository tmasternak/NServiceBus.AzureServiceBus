namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;
    using Microsoft.ServiceBus.Messaging;

    static class BrokeredMessageExtensions
    {
        public static async Task<bool> SafeCompleteAsync(this BrokeredMessage msg)
        {
            try
            {
                await msg.CompleteAsync();
                return true;
            }
            catch (MessageLockLostException ex)
            {
                // It's too late to compensate the loss of a message lock. We should just ignore it so that it does not break the receive loop.
                Log.Warn($"A message lock lost exception occured while trying to complete a message, you may consider to increase the lock duration or reduce the batch size, the exception was {ex.Message}", ex);
            }
            catch (MessagingException ex)
            {
                // There is nothing we can do as the connection may have been lost, or the underlying queue may have been removed.
                // If Complete() fails with this exception, the only recourse is to receive another message.
                Log.Warn($"A messaging exception occured while trying to complete a message, this might imply that the connection was lost or the underlying queue got removed, the exception was {ex.Message}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                // There is nothing we can do as the object has already been disposed elsewhere
                Log.Warn($"An object disposed exception occured while trying to complete a message, this might imply that the connection was lost or the underlying queue got removed, the exception was {ex.Message}", ex);
            }
            catch (TransactionException ex)
            {
                // ASB Sdk beat us to it
                Log.Warn($"A transaction exception occured while trying to complete a message, this probably means that the Azure ServiceBus SDK has rolled back the transaction already, the exception was {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                // took to long
                Log.Warn($"A timeout exception occured while trying to complete a message, the exception was {ex.Message}", ex);
            }
            return false;
        }

        public static async Task<bool> SafeAbandonAsync(this BrokeredMessage msg)
        {
            try
            {
                await msg.AbandonAsync();
                return true;
            }
            catch (MessageLockLostException ex)
            {
                // It's too late to compensate the loss of a message lock. We should just ignore it so that it does not break the receive loop.
                Log.Warn($"A message lock lost exception occured while trying to abandon a message, you may consider to increase the lock duration or reduce the batch size, the exception was {ex.Message}", ex);
            }
            catch (MessagingException ex)
            {
                // There is nothing we can do as the connection may have been lost, or the underlying queue may have been removed.
                // If Abandon() fails with this exception, the only recourse is to receive another message.
                Log.Warn($"A messaging exception occured while trying to abandon a message, this might imply that the connection was lost or the underlying queue got removed, the exception was {ex.Message}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                // There is nothing we can do as the object has already been disposed elsewhere
                Log.Warn($"An object disposed exception occured while trying to abandon a message, this might imply that the connection was lost or the underlying queue got removed, the exception was {ex.Message}", ex);
            }
            catch (TransactionException ex)
            {
                // ASB Sdk beat us to it
                Log.Warn($"A transaction exception occured while trying to abandon a message, this probably means that the Azure ServiceBus SDK has rolled back the transaction already, the exception was {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                // took to long
                Log.Warn($"A timeout exception occured while trying to abandon a message, the exception was {ex.Message}", ex);
            }
            return false;
        }

        public static BrokeredMessage CloneWithMessageId(this BrokeredMessage toSend)
        {
            var clone = toSend.Clone();
            clone.MessageId = toSend.MessageId;
            toSend = clone;
            return toSend;
        }

        public static IEnumerable<BrokeredMessage> CloneWithMessageId(this IEnumerable<BrokeredMessage> toSend)
        {
            return toSend.Select(message => message.CloneWithMessageId());
        }

        static ILog Log = LogManager.GetLogger(typeof(BrokeredMessageExtensions));
    }
}