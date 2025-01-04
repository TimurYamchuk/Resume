using System;

namespace Resume
{
    /// <summary>
    /// Модель резюме, содержащая основные данные о пользователе.
    /// </summary>
    public class ResumeModel
    {
        /// <summary>
        /// ФИО пользователя.
        /// </summary>
        public string UserFullName { get; set; }

        /// <summary>
        /// Возраст пользователя.
        /// </summary>
        public int UserAge { get; set; }

        /// <summary>
        /// Семейное положение пользователя.
        /// </summary>
        public string MaritalStat { get; set; }

        /// <summary>
        /// Адрес пользователя.
        /// </summary>
        public string UserAddress { get; set; }

        /// <summary>
        /// Электронная почта пользователя.
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// Навыки пользователя, перечисленные через запятую.
        /// </summary>
        public string UserSkills { get; set; }

        /// <summary>
        /// Преобразование модели в строковое представление.
        /// </summary>
        /// <returns>Строка, представляющая модель резюме, разделенную точкой с запятой.</returns>
        public override string ToString()
        {
            return $"{UserFullName};{UserAge};{MaritalStat};{UserAddress};{UserEmail};{UserSkills}";
        }

        /// <summary>
        /// Создание объекта ResumeModel из строки.
        /// </summary>
        /// <param name="line">Строка, содержащая данные о резюме, разделенные точкой с запятой.</param>
        /// <returns>Объект ResumeModel, если строка валидна; иначе null.</returns>
        public static ResumeModel FromLine(string line)
        {
            // Проверка пустоты строки или неправильного формата
            if (string.IsNullOrWhiteSpace(line)) return null;

            var parts = line.Split(';');

            // Проверка, что строка содержит все необходимые данные
            if (parts.Length != 6) return null;

            int parsedAge;
            bool isAgeValid = int.TryParse(parts[1], out parsedAge);

            // Возвращаем модель, если данные валидны
            return new ResumeModel
            {
                UserFullName = parts[0].Trim(),
                UserAge = isAgeValid ? parsedAge : 0, // Если возраст не может быть распознан, задаем 0
                MaritalStat = parts[2].Trim(),
                UserAddress = parts[3].Trim(),
                UserEmail = parts[4].Trim(),
                UserSkills = parts[5].Trim()
            };
        }
    }
}
