#include "StudentCharacter.h"

#include "Camera/CameraComponent.h"
#include "Components/CapsuleComponent.h"
#include "EnhancedInputComponent.h"
#include "EnhancedInputSubsystems.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "GameFramework/PlayerController.h"
#include "InputAction.h"
#include "InputMappingContext.h"
#include "Kismet/GameplayStatics.h"

AStudentCharacter::AStudentCharacter()
{
	PrimaryActorTick.bCanEverTick = false;

	// 角色基础朝向设置：由控制器旋转驱动摄像机，不让角色本体跟随俯仰/翻滚。
	bUseControllerRotationPitch = true;
	bUseControllerRotationYaw = true;
	bUseControllerRotationRoll = false;

	// 允许下蹲。
	GetCharacterMovement()->GetNavAgentPropertiesRef().bCanCrouch = true;

	// 初始化移动速度。
	GetCharacterMovement()->MaxWalkSpeed = WalkSpeed;
	GetCharacterMovement()->MaxWalkSpeedCrouched = CrouchSpeed;

	// 第一人称摄像机。
	FirstPersonCameraComponent = CreateDefaultSubobject<UCameraComponent>(TEXT("FirstPersonCameraComponent"));
	FirstPersonCameraComponent->SetupAttachment(GetCapsuleComponent());
	FirstPersonCameraComponent->SetRelativeLocation(FVector(0.0f, 0.0f, 64.0f));
	FirstPersonCameraComponent->bUsePawnControlRotation = true;
}

void AStudentCharacter::BeginPlay()
{
	Super::BeginPlay();

	// 将 Mapping Context 添加到本地玩家子系统。
	if (APlayerController* PC = Cast<APlayerController>(GetController()))
	{
		if (ULocalPlayer* LocalPlayer = PC->GetLocalPlayer())
		{
			if (UEnhancedInputLocalPlayerSubsystem* Subsystem = LocalPlayer->GetSubsystem<UEnhancedInputLocalPlayerSubsystem>())
			{
				if (DefaultMappingContext)
				{
					Subsystem->AddMappingContext(DefaultMappingContext, 0);
				}
			}
		}
	}

	UpdateMaxWalkSpeed();
}

void AStudentCharacter::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);

	if (UEnhancedInputComponent* EnhancedInputComponent = Cast<UEnhancedInputComponent>(PlayerInputComponent))
	{
		// WASD 移动。
		if (MoveAction)
		{
			EnhancedInputComponent->BindAction(MoveAction, ETriggerEvent::Triggered, this, &AStudentCharacter::Move);
		}

		// 鼠标视角。
		if (LookAction)
		{
			EnhancedInputComponent->BindAction(LookAction, ETriggerEvent::Triggered, this, &AStudentCharacter::Look);
		}

		// Shift 奔跑。
		if (RunAction)
		{
			EnhancedInputComponent->BindAction(RunAction, ETriggerEvent::Started, this, &AStudentCharacter::StartRun);
			EnhancedInputComponent->BindAction(RunAction, ETriggerEvent::Completed, this, &AStudentCharacter::StopRun);
			EnhancedInputComponent->BindAction(RunAction, ETriggerEvent::Canceled, this, &AStudentCharacter::StopRun);
		}

		// Ctrl 下蹲（按下切入，松开恢复）。
		if (CrouchAction)
		{
			EnhancedInputComponent->BindAction(CrouchAction, ETriggerEvent::Started, this, &AStudentCharacter::StartCrouch);
			EnhancedInputComponent->BindAction(CrouchAction, ETriggerEvent::Completed, this, &AStudentCharacter::StopCrouch);
			EnhancedInputComponent->BindAction(CrouchAction, ETriggerEvent::Canceled, this, &AStudentCharacter::StopCrouch);
		}
	}
}

void AStudentCharacter::Move(const FInputActionValue& Value)
{
	const FVector2D MoveInput = Value.Get<FVector2D>();
	if (Controller == nullptr)
	{
		return;
	}

	// 第一人称：按控制器 Yaw 方向在地面平面移动。
	const FRotator ControlRotation = Controller->GetControlRotation();
	const FRotator YawRotation(0.0f, ControlRotation.Yaw, 0.0f);

	const FVector ForwardDirection = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::X);
	const FVector RightDirection = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::Y);

	AddMovementInput(ForwardDirection, MoveInput.Y);
	AddMovementInput(RightDirection, MoveInput.X);
}

void AStudentCharacter::Look(const FInputActionValue& Value)
{
	const FVector2D LookInput = Value.Get<FVector2D>();

	AddControllerYawInput(LookInput.X);
	AddControllerPitchInput(LookInput.Y);
}

void AStudentCharacter::StartRun()
{
	bIsRunning = true;
	UpdateMaxWalkSpeed();
}

void AStudentCharacter::StopRun()
{
	bIsRunning = false;
	UpdateMaxWalkSpeed();
}

void AStudentCharacter::StartCrouch()
{
	Crouch();
	UpdateMaxWalkSpeed();
}

void AStudentCharacter::StopCrouch()
{
	UnCrouch();
	UpdateMaxWalkSpeed();
}

void AStudentCharacter::UpdateMaxWalkSpeed()
{
	if (!GetCharacterMovement())
	{
		return;
	}

	GetCharacterMovement()->MaxWalkSpeedCrouched = CrouchSpeed;

	if (bIsCrouched)
	{
		GetCharacterMovement()->MaxWalkSpeed = CrouchSpeed;
	}
	else
	{
		GetCharacterMovement()->MaxWalkSpeed = bIsRunning ? RunSpeed : WalkSpeed;
	}
}
