using System.Text.RegularExpressions;

namespace UT4MasterServer.Common.Helpers;

public static class ValidationHelper
{
	private static readonly Regex regexEmail;
	private static readonly List<string> disallowedUsernameWords;
	private static readonly List<string> disallowedEmailDomains;

	static ValidationHelper()
	{
		// regex from: https://www.emailregex.com/
		regexEmail = new Regex(@"^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])");
		disallowedUsernameWords = new List<string>
		{
			"cock", "dick", "penis", "vagina", "tits", "pussy", "boner",
			"shit", "fuck", "bitch", "slut", "sex", "cum",
			"nigger", "hitler", "nazi"
		};
		disallowedEmailDomains = new List<string>
		{
			"yopmail.com",
			"maildrop.cc",
			"dispostable.com",
			"guerrillamail.com",
			"mailinator.com",
			"tempr.email",
			"discard.email",
			"discardmail.com",
			"discardmail.de",
			"spambog.com",
			"spambog.de",
			"spambog.ru",
			"0815.ru",
			"knol-power.nl",
			"freundin.ru",
			"smashmail.de",
			"s0ny.net",
			"1mail.x24hr.com",
			"from.onmypc.info",
			"now.mefound.com",
			"mowgli.jungleheart.com",
			"cr.cloudns.asia",
			"tls.cloudns.asia",
			"msft.cloudns.asia",
			"b.cr.cloudns.asia",
			"ssl.tls.cloudns.asia",
			"sweetxxx.de",
			"dvd.dns-cloud.net",
			"dvd.dnsabr.com",
			"bd.dns-cloud.net",
			"yx.dns-cloud.net",
			"shit.dns-cloud.net",
			"shit.dnsabr.com",
			"eu.dns-cloud.net",
			"eu.dnsabr.com",
			"asia.dnsabr.com",
			"8.dnsabr.com",
			"pw.8.dnsabr.com",
			"mm.8.dnsabr.com",
			"23.8.dnsabr.com",
			"pw.epac.to",
			"postheo.de",
			"sexy.camdvr.org",
			"888.dns-cloud.net",
			"adult-work.info",
			"trap-mail.de",
			"m.cloudns.cl",
			"t.woeishyang.com",
			"pflege-schoene-haut.de",
			"streamboost.xyz",
			"okmail.p-e.kr",
			"hotbird.giize.com",
			"as10.dnsfree.com",
			"mehr-bitcoin.de",
			"a1b2.cloudns.ph",
			"wacamole.soynashi.tk",
			"temp69.email",
			"secure.okay.email.safeds.tk",
			"tajba.com",
			"web.run.place",
			"tempr-mail.line.pm",
			"spam.ceo",
			"healthydevelopimmune.com",
			"infobisnisdigital.com",
			"winayabeauty.com",
			"nxyl.eu",
			"edukansassu12a.cf",
			"mail.checkermaker.me",
			"mailcatch.com",
			"emailondeck.com",
			"mailnesia.com",
			"superrito.com",
			"armyspy.com",
			"cuvox.de",
			"dayrep.com",
			"einrot.com",
			"fleckens.hu",
			"gustr.com",
			"jourrapide.com",
			"rhyta.com",
			"superrito.com",
			"teleworm.us",
			"mintemail.com",
			"bbitq.com",
			"iucake.com",
			"gufum.com",
			"boxmail.lol",
			"nfstripss.com",
			"dropjar.com",
			"33mail.com",
		};
	}

	public static bool ValidateEmail(string email)
	{
		if (email.Length < 6 || email.Length > 64)
		{
			return false;
		}

		if (!regexEmail.IsMatch(email))
			return false;

		var emailDomain = email.ToLower().Split('@')[1];
		if (disallowedEmailDomains.Contains(emailDomain))
			return false;

		return true;
	}

	public static bool ValidateUsername(string username)
	{
		if (username.Length < 3 || username.Length > 32)
		{
			return false;
		}

		username = username.ToLower();

		// try to prevent impersonation of authority
		if (username == "admin" || username == "administrator" || username == "system")
		{
			return false;
		}

		// there's no way to prevent people from getting highly creative.
		// we just try some minimal filtering for now...
		foreach (var word in disallowedUsernameWords)
		{
			if (username.Contains(word))
			{
				return false;
			}
		}
		return true;
	}

	public static bool ValidatePassword(string password)
	{
		// we are expecting password to be SHA512 hash (64 bytes) in hex string form (128 chars)
		if (password.Length != 128)
		{
			return false;
		}

		if (!password.IsHexString())
		{
			return false;
		}

		return true;
	}
}
