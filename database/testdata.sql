use Nop

delete from Zgloszenie_Szczepionki
delete from Szczepionki_Odczyny
delete from Atrybuty_Zgloszenia
delete from Decyzje_Lekarza
delete from Atrybuty_Odczynow
delete from Odczyny_Zgloszenia
delete from Szczepionki
delete from Odczyny
delete from Zgloszenia
delete from Pacjenci
delete from Uzytkownicy

set identity_insert Uzytkownicy on

insert into Uzytkownicy 
	(id, imie, nazwisko, rola, "login", haslo,email)
values
	(0, 'Jan', 'Kowalski', 0, 'rodzic', 'rodzic','rodzic@wp.pl'),
	(1, 'Pracownik', 'Medyczny', 1, 'lekarz', 'lekarz','lekarz@wp.pl'),
	(2, 'Pracownik', 'PZH', 2, 'pzh', 'pzh','pzh@wp.pl')

set identity_insert Uzytkownicy off

set identity_insert Pacjenci on

insert into Pacjenci
	(id, imie, nazwisko, data_urodzenia, uzyt_id, lekarz_id)
values
	(0, 'Janek', 'Kowalski', '20100301', 0, 1)
	
set identity_insert Pacjenci off

set identity_insert Zgloszenia on

insert into Zgloszenia
	(id, uzyt_id, "data", zdjecie_ks_zd, lekarz_id, prosba_o_kontakt, pacjent_id)
values
	(0, 0, GETDATE(), './zdjecie.png', 1, 1, 0)

set identity_insert Zgloszenia off

set identity_insert Szczepionki on

insert into Szczepionki
	(id, nazwa, opis)
values
	(0, 'Menveo', 'Produkt Menveo jest przeznaczony do czynnego uodpornienia dzieci (w wieku od 2 lat), m�odzie�y
i doros�ych nara�onych na kontakt z dwoinkami zapalenia opon m�zgowych (Neisseria meningitidis)
z grup serologicznych A, C, W135 i Y, w celu zapobiegania chorobie inwazyjnej.
Szczepionk� nale�y stosowa� zgodnie z obowi�zuj�cymi oficjalnymi zaleceniami.')

set identity_insert Szczepionki off

set identity_insert Odczyny on

insert into Odczyny
	(id, nazwa)
values
	(0, 'Gor�czka')

set identity_insert Odczyny off

set identity_insert Atrybuty_Odczynow on

insert into Atrybuty_Odczynow
	(id, odczyn_id, typ, nazwa, info)
values
	(0, 0, 0, 'Temperatura', 'Stopni Celsjusza')

set identity_insert Atrybuty_Odczynow off

insert into Szczepionki_Odczyny
	(szczepionka_id, odczyn_id, stopien_ciezkosci, czestosc)
values
	(0, 0, 0, 0)

set identity_insert Odczyny_Zgloszenia on

insert into Odczyny_Zgloszenia
	(id, odczyn_id, zgloszenie_id, "data")
values
	(0, 0, 0, GETDATE())

set identity_insert Odczyny_Zgloszenia off

insert into Atrybuty_Zgloszenia
	(odzg_id, atod_id, wartosc)
values
	(0, 0, '37.7')

insert into Zgloszenie_Szczepionki
	(zgloszenie_id, szczepionka_id)
values
	(0, 0)