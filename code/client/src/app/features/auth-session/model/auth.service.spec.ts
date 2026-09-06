import { HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '@environments/environment';
import { withDefaultTestingProviders } from '@shared/lib/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    sessionStorage.setItem('ucbhold_access', 'legacy-access');
    sessionStorage.setItem('ucbhold_refresh', 'legacy-refresh');
    TestBed.configureTestingModule(withDefaultTestingProviders());
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    sessionStorage.clear();
  });

  it('stores the user without browser-readable tokens', () => {
    service.setSession({ nombre: 'Ana', rol: 'docente' });

    expect(service.isLoggedIn()).toBeTrue();
    expect(service.getRole()).toBe('docente');
    expect(sessionStorage.getItem('ucbhold_access')).toBeNull();
    expect(sessionStorage.getItem('ucbhold_refresh')).toBeNull();
  });

  it('refreshes using only the HttpOnly cookie', () => {
    service.refreshSession().subscribe();

    const request = http.expectOne(`${environment.apiUrl}/api/auth/refresh`);
    expect(request.request.withCredentials).toBeTrue();
    expect(request.request.body).toEqual({});
    request.flush({ Value: { Usuario: {} } });
  });

  it('clears local session data after logout', () => {
    service.setSession({ nombre: 'Ana', rol: 'docente' });
    service.logout().subscribe();

    const request = http.expectOne(`${environment.apiUrl}/api/auth/logout`);
    expect(request.request.withCredentials).toBeTrue();
    request.flush(null);
    expect(service.isLoggedIn()).toBeFalse();
  });
});
