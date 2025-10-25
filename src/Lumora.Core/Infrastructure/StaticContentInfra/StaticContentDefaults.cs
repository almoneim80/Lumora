namespace Lumora.Infrastructure.StaticContentInfra
{
    public static class StaticContentDefaults
    {
        private static readonly IReadOnlyDictionary<string, string> _arabic = new Dictionary<string, string>
        {
        { StaticContentKeys.HomePage.HeroTitle, "ندعمك ونساعدك" },
        { StaticContentKeys.HomePage.HeroDescription, "منصة UP لريادة الأعمال وتطوير المشاريع" },
        { StaticContentKeys.HomePage.BannerImage, "/images/home/banner.jpg" },
        { StaticContentKeys.HomePage.ServicesTitle, "ما الذي يمكننا أن نقدمه لك؟" },

        { StaticContentKeys.BusinessClub.Title, "نادي رواد الأعمال" },
        { StaticContentKeys.BusinessClub.Description, "هذا النص هو مثال لنص يمكن أن يستبدل في نفس المساحة." },

        { StaticContentKeys.Projects.LaunchSectionTitle, "قسم إطلاق المشاريع" },
        { StaticContentKeys.Projects.LaunchSectionDescription, "ابدأ مشروعك الخاص معنا بخطوات احترافية ومدروسة." },

        { StaticContentKeys.Footer.About, "منصة UP تهدف إلى دعم رواد الأعمال وتحفيز الابتكار." },
        { StaticContentKeys.Footer.Contact, "اتصل بنا لأي استفسار أو دعم فني." },
        { StaticContentKeys.Footer.Copyright, "© جميع الحقوق محفوظة لمنصة UP" },

        { StaticContentKeys.General.WelcomeMessage, "مرحبًا بكم في منصتنا" },
        { StaticContentKeys.General.PlatformSlogan, "تسوق معنا بأرخص الأسعار" }
        };

        private static readonly IReadOnlyDictionary<string, string> _english = new Dictionary<string, string>
        {
        { StaticContentKeys.HomePage.HeroTitle, "We support and assist you" },
        { StaticContentKeys.HomePage.HeroDescription, "UP is a platform for entrepreneurship and project development." },
        { StaticContentKeys.HomePage.BannerImage, "/images/home/banner.jpg" },
        { StaticContentKeys.HomePage.ServicesTitle, "What can we offer you?" },

        { StaticContentKeys.BusinessClub.Title, "Business Club" },
        { StaticContentKeys.BusinessClub.Description, "This is a placeholder text that can be replaced in the same space." },

        { StaticContentKeys.Projects.LaunchSectionTitle, "Project Launch Section" },
        { StaticContentKeys.Projects.LaunchSectionDescription, "Start your own project with us in a professional way." },

        { StaticContentKeys.Footer.About, "UP aims to support entrepreneurs and foster innovation." },
        { StaticContentKeys.Footer.Contact, "Contact us for inquiries or technical support." },
        { StaticContentKeys.Footer.Copyright, "© All rights reserved to UP platform" },

        { StaticContentKeys.General.WelcomeMessage, "Welcome to our platform" },
        { StaticContentKeys.General.PlatformSlogan, "Shop with us at the best prices" }
        };

        public static string? Get(string key, string language = "ar")
        {
            return language.ToLower() switch
            {
                "en" => _english.GetValueOrDefault(key),
                _ => _arabic.GetValueOrDefault(key)
            };
        }

        public static IEnumerable<KeyValuePair<string, string>> GetAll(string language = "ar")
        {
            return language.ToLower() == "en"
                ? _english
                : _arabic;
        }
    }
}
