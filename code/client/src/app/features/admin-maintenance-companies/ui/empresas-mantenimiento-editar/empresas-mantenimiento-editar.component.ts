import {
  Component,
  EventEmitter,
  HostListener,
  Input,
  Output,
  signal,
  WritableSignal,
} from '@angular/core';
import { ValidatedFormsModule } from '@shared/lib/forms';
import { EmpresaMantenimiento } from '@entities/admin';
import { EmpresamantenimientoService } from '@entities/maintenance-company';
import { BaseTablaComponent } from '@shared/lib/admin-table';
import { extractErrorMessage } from '@shared/lib/error';
import { Aviso, MostrarerrorComponent, ToastService } from '@shared/ui';
@Component({
  selector: 'app-empresas-mantenimiento-editar',
  imports: [
    ValidatedFormsModule,
    MostrarerrorComponent,
    Aviso,
  ],
  templateUrl: './empresas-mantenimiento-editar.component.html',
  styleUrl: './empresas-mantenimiento-editar.component.css',
})
export class EmpresasMantenimientoEditarComponent extends BaseTablaComponent {
  @Input() botoneditar: WritableSignal<boolean> = signal(true);
  @Output() actualizar: EventEmitter<void> = new EventEmitter<void>();
  @Input() empresaMantenimiento: EmpresaMantenimiento =
    new EmpresaMantenimiento();
  constructor(
    private readonly empresaMantenimientoapi: EmpresamantenimientoService,
    private readonly toast: ToastService,
  ) {
    super();
  }
  validaredicion() {
    this.mensajeaviso = '¿Desea guardar los cambios realizados?';
    this.aviso.set(true);
  }
  confirmar() {
    if (!this.iniciarEnvio()) return;
    this.empresaMantenimientoapi
      .actualizarEmpresaMantenimiento(this.empresaMantenimiento)
      .subscribe({
        next: () => {
          this.actualizar.emit();
          this.finalizarEnvio();
          this.toast.success(
            'Empresa de mantenimiento actualizada exitosamente.',
          );
          this.cerrar();
        },
        error: (error) => {
          const errorMsg = extractErrorMessage(
            error,
            'Error al actualizar la Empresa de Mantenimiento.',
          );
          this.mensajeerror = errorMsg;
          this.error.set(true);
          this.finalizarEnvio();
        },
      });
  }
  cerrar() {
    this.botoneditar.set(false);
  }
  @HostListener('click', ['$event'])
  onOverlayClick(event: MouseEvent) {
    if (event.target === event.currentTarget) this.cerrar();
  }
}
