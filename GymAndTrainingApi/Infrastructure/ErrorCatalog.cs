using Gym.Domain.Common.Errors;

namespace Gym.Api.Infrastructure
{
    public sealed record ErrorDescriptor(int StatusCode, string Title);

    public static class ErrorCatalog
    {
        private static readonly IReadOnlyDictionary<string, ErrorDescriptor> Errors =
            new Dictionary<string, ErrorDescriptor>(StringComparer.Ordinal)
            {
                [ErrorCodes.ValidationFailed] = new(400, "Помилка валідації даних"),
                [ErrorCodes.RequestInvalid] = new(400, "Некоректний запит"),
                [ErrorCodes.RequestMalformed] = new(400, "Некоректний формат запиту"),
                [ErrorCodes.ResourceNotFound] = new(404, "Ресурс не знайдено"),
                [ErrorCodes.MemberNotFound] = new(404, "Члена не знайдено"),
                [ErrorCodes.TrainerNotFound] = new(404, "Тренера не знайдено"),
                [ErrorCodes.BookingNotFound] = new(404, "Бронювання не знайдено"),
                [ErrorCodes.PaymentNotFound] = new(404, "Платіж не знайдено"),
                [ErrorCodes.EmailDuplicate] = new(409, "Email вже використовується"),
                [ErrorCodes.EmailInvalid] = new(400, "Некоректний email"),
                [ErrorCodes.MemberBanned] = new(409, "Член заблокований"),
                [ErrorCodes.MemberNameInvalid] = new(400, "Некоректне ім'я"),
                [ErrorCodes.MemberBodyMetricsInvalid] = new(400, "Некоректні метрики"),
                [ErrorCodes.SubscriptionDurationInvalid] = new(400, "Некоректна тривалість підписки"),
                [ErrorCodes.SubscriptionPlanUnavailable] = new(400, "План недоступний"),
                [ErrorCodes.SubscriptionCoverageExceeded] = new(409, "Перевищено ліміт покриття"),
                [ErrorCodes.SubscriptionPeriodConflict] = new(409, "Конфлікт періодів підписки"),
                [ErrorCodes.SubscriptionRequired] = new(409, "Потрібна активна підписка"),
                [ErrorCodes.PriceInvalid] = new(400, "Некоректна ціна"),
                [ErrorCodes.CurrencyInvalid] = new(400, "Некоректна валюта"),
                [ErrorCodes.PaymentInvalid] = new(400, "Некоректний платіж"),
                [ErrorCodes.BookingTimeWindowInvalid] = new(400, "Некоректний час бронювання"),
                [ErrorCodes.BookingAlreadyCompleted] = new(409, "Бронювання вже завершено"),
                [ErrorCodes.BookingCancellationTooLate] = new(409, "Пізно скасовувати бронювання"),
                [ErrorCodes.BookingCannotComplete] = new(409, "Неможливо завершити бронювання"),
                [ErrorCodes.BookingCannotCancel] = new(409, "Неможливо скасувати бронювання"),
                [ErrorCodes.BookingSlotTaken] = new(409, "Цей час уже зайнятий"),
                [ErrorCodes.BookingNotOwned] = new(403, "Бронювання належить іншому члену"),
                [ErrorCodes.TrainerInactive] = new(409, "Тренер неактивний"),
                [ErrorCodes.TrainerAlreadyInactive] = new(409, "Тренер уже неактивний"),
                [ErrorCodes.TrainerNameInvalid] = new(400, "Некоректне ім'я тренера"),
                [ErrorCodes.TrainerSpecializationInvalid] = new(400, "Некоректна спеціалізація"),
                [ErrorCodes.AuthUnauthorized] = new(401, "Не авторизовано"),
                [ErrorCodes.AuthForbidden] = new(403, "Доступ заборонено"),
                [ErrorCodes.ConcurrencyConflict] = new(409, "Конфлікт змін"),
                [ErrorCodes.DataConflict] = new(409, "Конфлікт даних"),
                [ErrorCodes.DataTransient] = new(503, "Тимчасова помилка даних"),
                [ErrorCodes.IdempotencyConflict] = new(409, "Конфлікт ідемпотентності"),
                [ErrorCodes.IdempotencyKeyRequired] = new(400, "Потрібен Idempotency-Key"),
                [ErrorCodes.RequestCancelled] = new(503, "Запит скасовано"),
                [ErrorCodes.ServerError] = new(500, "Внутрішня помилка сервера")
            };

        public static ErrorDescriptor Get(string code) =>
            Errors.TryGetValue(code, out var descriptor) ? descriptor : Errors[ErrorCodes.ServerError];

        public static string TypeFor(string code) => $"https://api.gym/errors/{code}";
    }
}
