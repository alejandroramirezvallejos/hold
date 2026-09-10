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
import { Gaveteros, Muebles } from '@entities/admin';
import { MuebleService } from '@entities/furniture';
import { GaveteroService } from '@entities/locker';
import { BaseTablaComponent } from '@shared/lib/admin-table';
import { extractErrorMessage } from '@shared/lib/error';
import {
  Aviso,
  CustomSelectComponent,
  MostrarerrorComponent,
  ToastService,
} from '@shared/ui';
@Component({
  selector: 'app-gaveteros-editar',
  imports: [
    ValidatedFormsModule,
    MostrarerrorComponent,
    Aviso,
    CustomSelectComponent,
  ],
  templateUrl: './gaveteros-editar.component.html',
  styleUrl: './gaveteros-editar.component.css',
})
export class GaveterosEditarComponent extends BaseTablaComponent {
  @Input() botoneditar: WritableSignal<boolean> = signal(true);
  @Output() actualizar: EventEmitter<void> = new EventEmitter<void>();
  @Input() gavetero: Gaveteros = new Gaveteros();
  muebles: string[] = [];
  constructor(
    private readonly gaveteroapi: GaveteroService,
    private mueblesAPI: MuebleService,
    private readonly toast: ToastService,
  ) {
    super();
  }
  ngOnInit() {
    this.cargarMuebles();
  }
  cargarMuebles() {
    this.mueblesAPI.obtenerMuebles().subscribe({
      next: (data: Muebles[]) => {
        this.muebles = data.map((mueble) => mueble.Nombre ?? '');
      },
      error: (error) => {
        const errorMsg = extractErrorMessage(
          error,
          'Error al cargar los muebles, intente mas tarde',
        );
        this.mensajeerror = errorMsg;
        this.error.set(true);
      },
    });
  }
  validaredicion() {
    this.mensajeaviso = '¿Desea confirmar la edición del gavetero?';
    this.aviso.set(true);
  }
  confirmar() {
    if (!this.iniciarEnvio()) return;
    this.gaveteroapi.editarGavetero(this.gavetero).subscribe({
      next: () => {
        this.actualizar.emit();
        this.finalizarEnvio();
        this.toast.success('Gavetero editado con éxito.');
        this.cerrar();
      },
      error: (error) => {
        const errorMsg = extractErrorMessage(
          error,
          'Error al editar el gavetero, intente mas tarde',
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
