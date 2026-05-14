#include "StudentPlayerCharacter.h"

#include "Camera/CameraComponent.h"
#include "Components/CapsuleComponent.h"
#include "EnhancedInputComponent.h"
#include "EnhancedInputSubsystems.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "GameFramework/PlayerController.h"
#include "InputAction.h"
#include "InputMappingContext.h"

AStudentPlayerCharacter::AStudentPlayerCharacter()
{
	PrimaryActorTick.bCanEverTick = false;

	// 第一人称：由控制器控制视角。
	bUseControllerRotationPitch = true;
	bUseControllerRotationYaw = true;
	bUseControllerRotationRoll = false;

	// 允许下蹲。
	GetCharacterMovement()->GetNavAgentPropertiesRef().bCanCrouch = true;

	// 初始化速度。
	GetCharacterMovement()->MaxWalkSpeed = WalkSpeed;
	GetCharacterMovement()->MaxWalkSpeedCrouched = CrouchSpeed;

	// 创建第一人称摄像机并挂在胶囊体上。
	FirstPersonCameraComponent = CreateDefaultSubobject<UCameraComponent>(TEXT("FirstPersonCameraComponent"));
	FirstPersonCameraComponent->SetupAttachment(GetCapsuleComponent());
	FirstPersonCameraComponent->SetRelativeLocation(FVector(0.0f, 0.0f, 64.0f));
	FirstPersonCameraComponent->bUsePawnControlRotation = true;
}

void AStudentPlayerCharacter::BeginPlay()
{
	Super::BeginPlay();

	// 将 Mapping Context 注入本地玩家输入子系统。
	if (APlayerController* PC = Cast<APlayerController>(GetController()))
	{
		if (ULocalPlayer* LocalPlayer = PC->GetLocalPlayer())
		{
			if (UEnhancedInputLocalPlayerSubsystem* InputSubsystem = LocalPlayer->GetSubsystem<UEnhancedInputLocalPlayerSubsystem>())
			{
				if (DefaultMappingContext)
				{
					InputSubsystem->AddMappingContext(DefaultMappingContext, 0);
				}
			}
		}
	}

	RefreshMovementSpeed();
}

void AStudentPlayerCharacter::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);

	if (UEnhancedInputComponent* EnhancedInputComp = Cast<UEnhancedInputComponent>(PlayerInputComponent))
	{
		if (MoveAction)
		{
			EnhancedInputComp->BindAction(MoveAction, ETriggerEvent::Triggered, this, &AStudentPlayerCharacter::HandleMove);
		}

		if (LookAction)
		{
			EnhancedInputComp->BindAction(LookAction, ETriggerEvent::Triggered, this, &AStudentPlayerCharacter::HandleLook);
		}

		if (RunAction)
		{
			EnhancedInputComp->BindAction(RunAction, ETriggerEvent::Started, this, &AStudentPlayerCharacter::StartRun);
			EnhancedInputComp->BindAction(RunAction, ETriggerEvent::Completed, this, &AStudentPlayerCharacter::StopRun);
			EnhancedInputComp->BindAction(RunAction, ETriggerEvent::Canceled, this, &AStudentPlayerCharacter::StopRun);
		}

		if (CrouchAction)
		{
			EnhancedInputComp->BindAction(CrouchAction, ETriggerEvent::Started, this, &AStudentPlayerCharacter::StartCrouch);
			EnhancedInputComp->BindAction(CrouchAction, ETriggerEvent::Completed, this, &AStudentPlayerCharacter::StopCrouch);
			EnhancedInputComp->BindAction(CrouchAction, ETriggerEvent::Canceled, this, &AStudentPlayerCharacter::StopCrouch);
		}
	}
}

void AStudentPlayerCharacter::HandleMove(const FInputActionValue& Value)
{
	const FVector2D MoveValue = Value.Get<FVector2D>();
	if (!Controller)
	{
		return;
	}

	const FRotator ControlRotation = Controller->GetControlRotation();
	const FRotator YawRotation(0.0f, ControlRotation.Yaw, 0.0f);

	const FVector ForwardVector = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::X);
	const FVector RightVector = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::Y);

	AddMovementInput(ForwardVector, MoveValue.Y);
	AddMovementInput(RightVector, MoveValue.X);
}

void AStudentPlayerCharacter::HandleLook(const FInputActionValue& Value)
{
	const FVector2D LookValue = Value.Get<FVector2D>();
	AddControllerYawInput(LookValue.X);
	AddControllerPitchInput(LookValue.Y);
}

void AStudentPlayerCharacter::StartRun()
{
	bWantsToRun = true;
	RefreshMovementSpeed();
}

void AStudentPlayerCharacter::StopRun()
{
	bWantsToRun = false;
	RefreshMovementSpeed();
}

void AStudentPlayerCharacter::StartCrouch()
{
	Crouch();
	RefreshMovementSpeed();
}

void AStudentPlayerCharacter::StopCrouch()
{
	UnCrouch();
	RefreshMovementSpeed();
}

void AStudentPlayerCharacter::RefreshMovementSpeed()
{
	UCharacterMovementComponent* MovementComp = GetCharacterMovement();
	if (!MovementComp)
	{
		return;
	}

	MovementComp->MaxWalkSpeedCrouched = CrouchSpeed;

	if (bIsCrouched)
	{
		MovementComp->MaxWalkSpeed = CrouchSpeed;
	}
	else
	{
		MovementComp->MaxWalkSpeed = bWantsToRun ? RunSpeed : WalkSpeed;
	}
}
