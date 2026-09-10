import {
  Component,
  EventEmitter,
  HostListener,
  Input,
  OnChanges,
  Output,
  signal,
  WritableSignal,
} from '@angular/core';
import { ValidatedFormsModule } from '@shared/lib/forms';
import { GrupoEquipo, GrupoequipoService } from '@entities/equipment-group';
import { BaseTablaComponent } from '@shared/lib/admin-table';
import { extractErrorMessage } from '@shared/lib/error';
import {
  Aviso,
  CustomSelectComponent,
  MostrarerrorComponent,
  ToastService,
} from '@shared/ui';
@Component({
  selector: 'app-grupos-equipos-editar',
  standalone: true,
  imports: [
    ValidatedFormsModule,
    MostrarerrorComponent,
    Aviso,
    CustomSelectComponent,
  ],
  templateUrl: './grupos-equipos-editar.component.html',
  styleUrl: './grupos-equipos-editar.component.css',
})
export class GruposEquiposEditarComponent
  extends BaseTablaComponent
  implements OnChanges
{
  @Input() botoneditar: WritableSignal<boolean> = signal(true);
  @Output() actualizar: EventEmitter<void> = new EventEmitter<void>();
  @Input() categorias: string[] = [];
  @Input() grupoequipo: GrupoEquipo = new GrupoEquipo();
  grupoEquipo: GrupoEquipo = { ...this.grupoequipo };
  constructor(
    private readonly grupoEquipoapi: GrupoequipoService,
    private readonly toast: ToastService,
  ) {
    super();
  }
  ngOnChanges() {
    this.grupoEquipo = { ...this.grupoequipo };
  }
  validaredicion() {
    this.mensajeaviso =
      '¿Desea guardar los cambios realizados al grupo de equipo?';
    this.aviso.set(true);
  }
  confirmar() {
    if (!this.iniciarEnvio()) return;
    this.grupoEquipoapi.editarGrupoEquipo(this.grupoEquipo).subscribe({
      next: (_response) => {
        this.cerrar();
        this.finalizarEnvio();
        this.toast.success('Grupo de equipo editado exitosamente.');
        this.actualizar.emit();
      },
      error: (error) => {
        const errorMsg = extractErrorMessage(
          error,
          'Error al editar el grupo de equipo',
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
